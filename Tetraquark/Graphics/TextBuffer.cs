﻿using SFML.Graphics;
using SFML.System;

namespace Tq.Graphics {
	class TextBuffer : Drawable {
		static Shader TextBufferShader = Shader.FromString(@"
#version 110

void main() {
	gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
	gl_TexCoord[0] = gl_TextureMatrix[0] * gl_MultiTexCoord0;
	gl_FrontColor = gl_Color;
}
", @"
#version 110

uniform sampler2D font;
uniform sampler2D foredata;
uniform sampler2D backdata;
uniform vec2 buffersize;
uniform vec4 fontsizes;

void main() {
	vec4 fore = texture2D(foredata, gl_TexCoord[0].xy);
	vec4 back = texture2D(backdata, gl_TexCoord[0].xy);
	float char = 255 * fore.a;
	
	vec2 fontpos = vec2(floor(mod(char, fontsizes.z)) * fontsizes.x, floor(char / fontsizes.w) * fontsizes.y);
	vec2 offset = vec2(mod(floor(gl_TexCoord[0].x * (buffersize.x * fontsizes.x)), fontsizes.x),
					   mod(floor(gl_TexCoord[0].y * (buffersize.y * fontsizes.y)), fontsizes.y));

	vec4 fontclr = texture2D(font, (fontpos + offset) / vec2(fontsizes.x * fontsizes.z, fontsizes.y * fontsizes.w));
	gl_FragColor = fontclr * vec4(fore.rgb, 1) + (1.0 - fontclr) * vec4(back.rgb, 1);
}
");

		public int BufferWidth {
			get {
				return W;
			}
		}

		public int BufferHeight {
			get {
				return H;
			}
		}

		public int CharWidth {
			get {
				return CharW;
			}
		}

		public int CharHeight {
			get {
				return CharH;
			}
		}

		public Sprite Sprite {
			get;
			private set;
		}

		int W, H, CharW, CharH;
		bool Dirty;
		RenderTexture RT;
		Vertex[] ScreenQuad;
		RenderStates TextStates;
		Texture ForeData, BackData, ASCIIFont;
		byte[] ForeDataRaw, BackDataRaw;

		public TextBuffer(uint W, uint H) {

			this.W = (int)W;
			this.H = (int)H;
			Dirty = true;
			CharW = 8;
			CharH = 12;

			ForeDataRaw = new byte[W * H * 4];
			ForeData = new Texture(new Image(W, H, ForeDataRaw));
			ForeData.Smooth = false;
			BackDataRaw = new byte[W * H * 4];
			BackData = new Texture(new Image(W, H, BackDataRaw));
			BackData.Smooth = false;

			RT = new RenderTexture(W * (uint)CharW, H * (uint)CharH);
			RT.Texture.Smooth = true;
			Sprite = new Sprite(RT.Texture);
			TextStates = new RenderStates(TextBufferShader);

			ScreenQuad = new Vertex[] {
				new Vertex(new Vector2f(0, 0), Color.Red, new Vector2f(0, 0)), 
				new Vertex(new Vector2f(RT.Size.X, 0), Color.Green, new Vector2f(1, 0)),
				new Vertex(new Vector2f(RT.Size.X, RT.Size.Y), Color.Blue, new Vector2f(1, 1)),
				new Vertex(new Vector2f(0, RT.Size.Y), Color.Black, new Vector2f(0, 1)),
			};
		}

		public void SetFontTexture(Texture Fnt, int CharW = 8, int CharH = 12) {
			this.CharW = CharW;
			this.CharH = CharH;
			ASCIIFont = Fnt;
			Dirty = true;
		}

		public void Set(int X, int Y, char C, Color Fg, Color Bg) {
			Set(Y * W + X, C, Fg, Bg);
		}

		public void Set(int X, int Y, Color Fg, Color Bg) {
			Set(Y * W + X, Fg, Bg);
		}

		public void Set(int Idx, Color Fg, Color Bg) {
			Idx *= 4;
			ForeDataRaw[Idx] = Fg.R;
			ForeDataRaw[Idx + 1] = Fg.G;
			ForeDataRaw[Idx + 2] = Fg.B;
			BackDataRaw[Idx] = Bg.R;
			BackDataRaw[Idx + 1] = Bg.G;
			BackDataRaw[Idx + 2] = Bg.B;
			Dirty = true;
		}

		public void Set(int Idx, char C, Color Fg, Color Bg) {
			Set(Idx, Fg, Bg);
			Idx *= 4;
			ForeDataRaw[Idx + 3] = (byte)C;
			BackDataRaw[Idx + 3] = 0;
			Dirty = true;
		}

		public void Clear(char C = (char)0) {
			Clear(C, Color.White, Color.Black);
		}

		public void Clear(char C, Color Fg, Color Bg) {
			for (int i = 0; i < W * H; i++)
				Set(i, C, Fg, Bg);
		}

		public void Print(int X, int Y, string Str) {
			Print(X, Y, Str, Color.White, Color.Black);
		}

		public void Print(int X, int Y, string Str, Color Fg, Color Bg) {
			Print(Y * W + X, Str, Fg, Bg);
		}

		public void Print(int I, string Str) {
			Print(I, Str, Color.White, Color.Black);
		}

		public void Print(int I, string Str, Color Fg, Color Bg) {
			for (int i = 0; i < Str.Length; i++)
				Set(I + i, Str[i], Fg, Bg);
		}

		void Update() {
			if (!Dirty)
				return;
			Dirty = false;
			ForeData.Update(ForeDataRaw);
			BackData.Update(BackDataRaw);

			TextStates.Shader.SetParameter("font", ASCIIFont);
			TextStates.Shader.SetParameter("foredata", ForeData);
			TextStates.Shader.SetParameter("backdata", BackData);
			TextStates.Shader.SetParameter("buffersize", W, H);
			TextStates.Shader.SetParameter("fontsizes", CharW, CharH, ASCIIFont.Size.X / CharW, ASCIIFont.Size.Y / CharH);

			RT.Clear(Color.Black);
			RT.Draw(ScreenQuad, PrimitiveType.Quads, TextStates);
			RT.Display();
		}

		public void Draw(RenderTarget R, RenderStates S) {
			Update();
			Sprite.Draw(R, S);
		}
	}
}
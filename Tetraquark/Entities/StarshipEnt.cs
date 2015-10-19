﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Physics;
using ChipmunkSharp;
using OpenTK.Graphics.OpenGL;
using SFML.System;
using SFML.Graphics;
using SFML.Window;
using PrimType = SFML.Graphics.PrimitiveType;
using Tq.Graphics;
using Tq.Entities;
using Tq.Game;

namespace Tq.Entities {
	class StarshipEnt : PhysicsEnt {
		internal RectangleShape Rect;

		public StarshipEnt(float W = 2, float H = 2) {
			Rect = new RectangleShape(new Vector2f(W, H));
			Rect.Origin = Rect.Size / 2;
			Rect.FillColor = Color.Red;

			Phys.CreateBoxPhysics(W, H, Density.Wood, this);
			Body.SetVelocityUpdateFunc((S, Dmp, Dt) => Phys.CenterGravity(Body, S, Dmp, Dt));
		}

		public override void Draw(RenderSprite RT) {
			/*Engine.ActiveCamera.Position = Position;
			Engine.ActiveCamera.Rotation = Angle;
			RT.SetView(Engine.ActiveCamera);*/

			Rect.Position = Position;
			Rect.Rotation = Angle;
			RT.Draw(Rect);
		}
	}
}
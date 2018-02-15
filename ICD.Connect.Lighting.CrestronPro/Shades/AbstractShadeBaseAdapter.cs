using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Lighting.Shades;
using ICD.Connect.Lighting.Shades.Controls;

namespace ICD.Connect.Lighting.CrestronPro.Shades
{
	public abstract class AbstractShadeBaseAdapter<TShade, TSettings> 
		: AbstractShadeDevice<TSettings>,
		IShadeBaseAdapter
		where TShade : ShadeBase
		where TSettings : IShadeBaseAdapterSettings, new()
	{
		public event EventHandler OnDirectionChanged;

		private eShadeDirection m_LastDirection = eShadeDirection.Neither;

		protected TShade Shade { get; private set; }

		public AbstractShadeBaseAdapter()
		{
			Controls.Add(new ShadeStopControl<IShadeBaseAdapter>(this, 0));
			Controls.Add(new ShadeSetPositionControl<IShadeBaseAdapter>(this, 1));
			Controls.Add(new ShadeInMotionFeedbackControl<IShadeBaseAdapter>(this, 2));
			Controls.Add(new ShadePositionFeedbackControl<IShadeBaseAdapter>(this, 3));
			Controls.Add(new ShadeLastDirectionControl<IShadeBaseAdapter>(this, 4));
		}

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return Shade != null && Shade.IsOnline;
		}

		protected void SetShade(TShade shade)
		{
			if (shade == Shade)
				return;

			Unsubscribe(Shade);

			if (Shade != null)
			{
				if (Shade.Registered)
					Shade.UnRegister();

				try
				{
					Shade.Dispose();
				}
				catch { }
			}

			Shade = shade;

			if (Shade != null && !Shade.Registered)
			{
				if (Name != null)
				{
					Shade.Description = Name;
				}
				eDeviceRegistrationUnRegistrationResponse result = Shade.Register();
				if (result != eDeviceRegistrationUnRegistrationResponse.Success)
					Logger.AddEntry(eSeverity.Error, "Unable to register {0} - {1}", Shade.GetType().Name, result);
			}

			Subscribe(Shade);
			UpdateCachedOnlineStatus();
		}

		protected virtual void Subscribe(TShade shade)
		{
			
		}

		protected virtual void Unsubscribe(TShade shade)
		{
			
		}

		public override void Open()
		{
			Shade.Open();
			if (m_LastDirection == eShadeDirection.Open)
				return;
			m_LastDirection = eShadeDirection.Open;
			OnDirectionChanged.Raise(this);
		}

		public override void Close()
		{
			Shade.Close();
			if (m_LastDirection == eShadeDirection.Close)
				return;
			m_LastDirection = eShadeDirection.Close;
			OnDirectionChanged.Raise(this);
		}

		public void Stop()
		{
			Shade.Stop();
		}

		public eShadeDirection GetLastDirection()
		{
			switch (Shade.LastDirection)
			{
				case eShadeMovement.NA:
					return eShadeDirection.Neither;
				case eShadeMovement.Opened:
					return eShadeDirection.Open;
				case eShadeMovement.Closed:
					return eShadeDirection.Close;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public bool GetIsInMotion()
		{
			return Shade.IsRaising.BoolValue || Shade.IsLowering.BoolValue;
		}

		public float GetPosition()
		{
			return MathUtils.MapRange(0, uint.MaxValue, 0, 1, Shade.PositionFeedback.UShortValue);
		}

		public void SetPosition(float position)
		{
			position = MathUtils.Clamp(position, 0, 1);
			float floatPosition = MathUtils.MapRange(0, 1, 0, ushort.MaxValue, position);
			Shade.SetPosition((ushort)floatPosition);
		}
	}
}
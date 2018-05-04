using ICD.Connect.Lighting.Shades;

namespace ICD.Connect.Lighting.Mock.Controls
{
	public sealed class MockLightingShadeGroupControl : AbstractMockShadeLightingControl
	{
		public override LightingProcessorControl.ePeripheralType PeripheralType
		{
			get { return LightingProcessorControl.ePeripheralType.ShadeGroup; }
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="room"></param>
		/// <param name="name"></param>
		/// <param name="shadeType"></param>
		public MockLightingShadeGroupControl(int id, int room, string name, eShadeType shadeType)
			: base(id, room, name, shadeType)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="room"></param>
		/// <param name="name"></param>
		public MockLightingShadeGroupControl(int id, int room, string name)
			: base(id, room, name, eShadeType.None)
		{
		}

		public override void Dispose()
		{
		}
	}
}

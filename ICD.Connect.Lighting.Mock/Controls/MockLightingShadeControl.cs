using ICD.Connect.Lighting.Shades;

namespace ICD.Connect.Lighting.Mock.Controls
{
	public sealed class MockLightingShadeControl : AbstractMockShadeLightingControl
	{
		public override LightingProcessorControl.ePeripheralType PeripheralType
		{
			get { return LightingProcessorControl.ePeripheralType.Shade; }
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="room"></param>
		/// <param name="name"></param>
		public MockLightingShadeControl(int id, int room, string name, eShadeType shadeType)
			: base(id, room, name, shadeType)
		{
		}

		public MockLightingShadeControl(int id, int room, string name)
			: base(id, room, name, eShadeType.None)
		{
		}

		public override void Dispose()
		{
			
		}
	}
}

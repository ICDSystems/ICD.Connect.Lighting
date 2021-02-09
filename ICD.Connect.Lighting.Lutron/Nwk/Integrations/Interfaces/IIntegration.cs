using System;
using ICD.Connect.API.Nodes;

namespace ICD.Connect.Lighting.Lutron.Nwk.Integrations.Interfaces
{
	public interface IIntegration<TIntegrationId>: IDisposable, IConsoleNode
	{
		string Name { get; }

		TIntegrationId IntegrationId { get; }
	}
}

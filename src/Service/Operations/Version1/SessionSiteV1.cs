using System.Runtime.Serialization;

namespace PipServices.Templates.Facade.Operations.Version1
{
	[DataContract]
	public class SessionSiteV1: ISessionSite
	{
		[DataMember(Name = "id")]
		public string Id { get; set; }
		[DataMember(Name = "name")]
		public string Name { get; set; }
	}
}

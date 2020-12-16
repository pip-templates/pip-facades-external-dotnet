using PipServices3.Commons.Config;
using PipServices3.Commons.Refer;
using PipServices3.Rpc.Services;
using PipServices.Templates.Facade.Operations.Version1;
using System.Dynamic;
using System.IO;
using PipServices3.Commons.Convert;
using Microsoft.Extensions.Primitives;
using System.Globalization;
using PipServices3.Components.Info;
using System;
using System.Text;
using Newtonsoft.Json;

namespace PipServices.Templates.Facade.Services.Version1
{
	public class FacadeServiceV1 : RestService
    {
		private AboutOperations _aboutOperations = new AboutOperations();
		private SessionsOperationsV1 _sessionsOperations = new SessionsOperationsV1();
		private InvitationsOperationsV1 _invitationsOperations = new InvitationsOperationsV1();
		private SitesOperationsV1 _sitesOperations = new SitesOperationsV1();
		private BeaconsOperationsV1 _beaconsOperations = new BeaconsOperationsV1();

		private ContextInfo _contextInfo;

		public FacadeServiceV1()
		{
			_baseRoute = "api/v1";
		}

		public override void Configure(ConfigParams config)
		{
			base.Configure(config);

			_aboutOperations.Configure(config);
			_sessionsOperations.Configure(config);
			_invitationsOperations.Configure(config);
			_sitesOperations.Configure(config);
			_beaconsOperations.Configure(config);
		}

		public override void SetReferences(IReferences references)
		{
			base.SetReferences(references);

			_aboutOperations.SetReferences(references);
			_sessionsOperations.SetReferences(references);
			_invitationsOperations.SetReferences(references);
			_sitesOperations.SetReferences(references);
			_beaconsOperations.SetReferences(references);

			_contextInfo = references.GetOneOptional<ContextInfo>(
				new Descriptor("pip-services", "context-info", "*", "*", "*"));
		}

		public override void Register()
        {
			var auth = new AuthorizerV1();

			// Restore session middleware
			RegisterInterceptor("", _sessionsOperations.LoadSessionAsync);

			// About Route
			RegisterRouteWithAuth("get", "/about", auth.Anybody(),
				async (request, response, user, routeData) => { await _aboutOperations.AboutAsync(request, response, user); });

			// Beacon Routes
			RegisterBeacons(auth);

			// Session Routes
			RegisterSession(auth);

			// Site Routes
			RegisterSite(auth);

			// Invitation Routes
			RegisterInvitation(auth);
		}

		private void RegisterInvitation(AuthorizerV1 auth)
		{
			RegisterRouteWithAuth("get", "/sites/{site_id}/invitations", auth.SiteUser(), _invitationsOperations.GetInvitationsAsync);
			RegisterRouteWithAuth("get", "/sites/{site_id}/invitations/{invitation_id}", auth.SiteUser(), _invitationsOperations.GetInvitationAsync);
			RegisterRouteWithAuth("post", "/sites/{site_id}/invitations", auth.Signed(), _invitationsOperations.SendInvitationAsync);
			RegisterRouteWithAuth("post", "/sites/{site_id}/invitations/notify", auth.SiteManager(), _invitationsOperations.NotifyInvitationAsync);
			RegisterRouteWithAuth("delete", "/sites/{site_id}/invitations/{invitation_id}", auth.SiteManager(), _invitationsOperations.DeleteInvitationAsync);
			RegisterRouteWithAuth("post", "/sites/{site_id}/invitations/{invitation_id}/approve", auth.SiteManager(), _invitationsOperations.ApproveInvitationAsync);
			RegisterRouteWithAuth("post", "/sites/{site_id}/invitations/{invitation_id}/deny", auth.SiteManager(), _invitationsOperations.DenyInvitationAsync);
			RegisterRouteWithAuth("post", "/sites/{site_id}/invitations/{invitation_id}/resend", auth.SiteManager(), _invitationsOperations.ResendInvitationAsync);
		}

		private void RegisterSite(AuthorizerV1 auth)
		{
			RegisterRouteWithAuth("get", "/sites", auth.Signed(), _sitesOperations.GetAuthorizedSitesAsync);
			RegisterRouteWithAuth("get", "/sites/all", auth.Admin(), _sitesOperations.GetSitesAsync);
			RegisterRouteWithAuth("get", "/sites/find_by_code", auth.Anybody(), _sitesOperations.FindSiteByCodeAsync);
			RegisterRouteWithAuth("get", "/sites/{site_id}", auth.SiteUser(), _sitesOperations.GetSiteAsync);
			RegisterRouteWithAuth("post", "/sites/{site_id}/generate_code", auth.SiteAdmin(), _sitesOperations.GenerateCodeAsync);
			RegisterRouteWithAuth("post", "/sites", auth.Signed(), _sitesOperations.CreateSiteAsync);
			RegisterRouteWithAuth("post", "/sites/validate_code", auth.Signed(), _sitesOperations.ValidateSiteCodeAsync);
			RegisterRouteWithAuth("put", "/sites/{site_id}", auth.SiteAdmin(), _sitesOperations.UpdateSiteAsync);
			RegisterRouteWithAuth("delete", "/sites/{site_id}", auth.Admin(), _sitesOperations.DeleteSiteAsync);
			RegisterRouteWithAuth("post", "/sites/{site_id}/remove", auth.SiteUser(), _sitesOperations.RemoveSiteAsync);
		}

		private void RegisterSession(AuthorizerV1 auth)
		{
			RegisterRouteWithAuth("post", "/signup", auth.Anybody(), _sessionsOperations.SignupAsync);
			RegisterRouteWithAuth("get", "/signup/validate", auth.Anybody(), _sessionsOperations.SignupValidateAsync);
			RegisterRouteWithAuth("post", "/signin", auth.Anybody(), _sessionsOperations.SigninAsync);
			RegisterRouteWithAuth("post", "/signout", auth.Anybody(), _sessionsOperations.SignoutAsync);
			RegisterRouteWithAuth("get", "/sessions", auth.Admin(),	_sessionsOperations.GetSessionsAsync);
			RegisterRouteWithAuth("post", "/sessions/restore", auth.Signed(), _sessionsOperations.RestoreSessionAsync);
			RegisterRouteWithAuth("get", "/sessions/current", auth.Signed(), _sessionsOperations.GetCurrentSessionAsync);
			RegisterRouteWithAuth("get", "/sessions/{user_id}", auth.OwnerOrAdmin("user_id"), _sessionsOperations.GetUserSessionsAsync);
			RegisterRouteWithAuth("delete", "/sessions/{user_id}/{session_id}", auth.OwnerOrAdmin("user_id"), _sessionsOperations.CloseSessionAsync);
		}

		private void RegisterBeacons(AuthorizerV1 auth)
		{
			RegisterRouteWithAuth("get", "/sites/{site_id}/beacons", auth.SiteUser(), _beaconsOperations.GetBeaconsAsync);
			RegisterRouteWithAuth("get", "/sites/{site_id}/beacons/{beacon_id}", auth.SiteUser(), _beaconsOperations.GetBeaconAsync);
			RegisterRouteWithAuth("post", "/sites/{site_id}/beacons/calculate_position", auth.SiteManager(), _beaconsOperations.CalculatePositionAsync);
			RegisterRouteWithAuth("post", "/sites/{site_id}/beacons", auth.SiteManager(), _beaconsOperations.CreateBeaconAsync);
			RegisterRouteWithAuth("post", "/sites/{site_id}/beacons/validate_udi", auth.Signed(), _beaconsOperations.ValidateBeaconUdiAsync);
			RegisterRouteWithAuth("put", "/sites/{site_id}/beacons/{beacon_id}", auth.SiteManager(), _beaconsOperations.UpdateBeaconAsync);
			RegisterRouteWithAuth("delete", "/sites/{site_id}/beacons/{beacon_id}", auth.SiteManager(),	_beaconsOperations.DeleteBeaconAsync);
		}
    }
}
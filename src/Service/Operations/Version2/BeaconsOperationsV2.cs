﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PipServices.Templates.Facade.Clients.Version1;
using PipServices3.Commons.Convert;
using PipServices3.Commons.Refer;
using PipServices3.Rpc.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PipServices.Templates.Facade.Operations.Version2
{
	public class BeaconsOperationsV2 : RestOperations
	{
		private IBeaconsClientV1 _beaconsClient;

		public BeaconsOperationsV2()
		{
			_dependencyResolver.Put("beacons", new Descriptor("pip-services-beacons", "client", "*", "*", "1.0"));
		}

		public new void SetReferences(IReferences references)
		{
			base.SetReferences(references);
			_beaconsClient = _dependencyResolver.GetOneRequired<IBeaconsClientV1>("beacons");
		}

		public async Task GetBeaconsXAsync(HttpRequest request, HttpResponse response, ClaimsPrincipal user,
			RouteData routeData)
		{
			var filter = GetFilterParams(request);
			var paging = GetPagingParams(request);
			var parameters = GetParameters(request);

			var siteId = parameters.GetAsString("site_id");
			filter.SetAsObject("site_id", siteId);

			var page = await _beaconsClient.GetBeaconsAsync(null, filter, paging);
			await SendResultAsync(response, page);
		}

		public async Task GetBeaconXAsync(HttpRequest request, HttpResponse response, ClaimsPrincipal user, RouteData routeData)
		{
			var parameters = GetParameters(request);
			var siteId = parameters.GetAsString("site_id");
			var beaconId = parameters.GetAsString("beacon_id");

			var beacon = await _beaconsClient.GetBeaconByIdAsync(null, beaconId);
			await SendResultAsync(response, beacon);
		}

		public async Task CalculatePositionXAsync(HttpRequest request, HttpResponse response, ClaimsPrincipal user, RouteData routeData)
		{
			var parameters = GetParameters(request);
			var siteId = parameters.GetAsString("site_id");
			var udis = parameters.GetAsString("udis")?.Split(',');

			var center = await _beaconsClient.CalculatePositionAsync(null, siteId, udis);
			await SendResultAsync(response, center);
		}

		public async Task CreateBeaconXAsync(HttpRequest request, HttpResponse response, ClaimsPrincipal user, RouteData routeData)
		{
			var parameters = GetParameters(request);
			var beacon = JsonConverter.FromJson<BeaconV1>(parameters.RequestBody);

			beacon = await _beaconsClient.CreateBeaconAsync(null, beacon);
			await SendResultAsync(response, beacon);
		}

		public async Task ValidateBeaconUdiXAsync(HttpRequest request, HttpResponse response, ClaimsPrincipal user, RouteData routeData)
		{
			var parameters = GetParameters(request);
			var udi = parameters.GetAsString("udi");

			var beacon = await _beaconsClient.GetBeaconByUdiAsync(null, udi);
			await SendResultAsync(response, beacon?.Id);
		}

		public async Task UpdateBeaconXAsync(HttpRequest request, HttpResponse response, ClaimsPrincipal user, RouteData routeData)
		{
			var parameters = GetParameters(request);
			var beacon = JsonConverter.FromJson<BeaconV1>(parameters.RequestBody);

			beacon = await _beaconsClient.UpdateBeaconAsync(null, beacon);
			await SendResultAsync(response, beacon);
		}

		public async Task DeleteBeaconXAsync(HttpRequest request, HttpResponse response, ClaimsPrincipal user, RouteData routeData)
		{
			var parameters = GetParameters(request);
			var beaconId = parameters.GetAsString("beacon_id");

			var beacon = await _beaconsClient.DeleteBeaconByIdAsync(null, beaconId);
			await SendResultAsync(response, beacon);
		}
	}
}
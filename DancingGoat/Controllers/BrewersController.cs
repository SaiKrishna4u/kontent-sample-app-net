﻿using DancingGoat.Models;
using Kentico.Kontent.Delivery;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Kentico.AspNetCore.LocalizedRouting.Attributes;
using Kentico.Kontent.Delivery.Abstractions;
using DancingGoat.Configuration;

namespace DancingGoat.Controllers
{
    [LocalizedRoute(CultureConstants.EnglishCulture, "Brewers")]
    [LocalizedRoute(CultureConstants.SpanishCulture, "Cerveceros")]
    public class BrewersController : ControllerBase
    {
        public BrewersController(IDeliveryClientFactory deliveryClientFactory) : base(deliveryClientFactory)
        {
        }

        [LocalizedRoute(CultureConstants.EnglishCulture, "Index")]
        [LocalizedRoute(CultureConstants.SpanishCulture, "Indice")]
        public async Task<ActionResult> Index()
        {
            var itemsTask = _client.GetItemsAsync<Brewer>(
                new OrderParameter("elements.product_name"),
                new ElementsParameter(Brewer.ImageCodename, Brewer.PriceCodename, Brewer.ProductStatusCodename, Brewer.ProductNameCodename, Brewer.UrlPatternCodename),
                new LanguageParameter(Language),
                new DepthParameter(0)
            );

            var statusTask = _client.GetTaxonomyAsync(Brewer.ProductStatusCodename);
            var manufacturerTask = _client.GetTaxonomyAsync(Brewer.ManufacturerCodename);

            var model = new BrewersViewModel
            {
                Items = (await itemsTask).Items,
                Filter = new BrewerFilterViewModel
                {
                    AvailableProductStatuses = GetTaxonomiesAsSelectList(await statusTask),
                    AvailableManufacturers = GetTaxonomiesAsSelectList(await manufacturerTask)
                }
            };

            return View(model);
        }

        [LocalizedRoute(CultureConstants.EnglishCulture, "Filter")]
        [LocalizedRoute(CultureConstants.SpanishCulture, "Filter")]
        public async Task<ActionResult> Filter(BrewerFilterViewModel model)
        {
            var parameters = new List<IQueryParameter> {
                new OrderParameter($"elements.{Brewer.ProductNameCodename}"),
                new ElementsParameter(Brewer.ImageCodename, Brewer.PriceCodename, Brewer.ProductStatusCodename, Brewer.ProductNameCodename, Brewer.UrlPatternCodename),
                new DepthParameter(0),
                new LanguageParameter(Language)
            };

            var manufacturers = model.GetFilteredManufacturers().ToArray();
            if (manufacturers.Any())
            {
                parameters.Add(new AnyFilter($"elements.{Brewer.ManufacturerCodename}", manufacturers));
            }

            var statusTypes = model.GetFilteredProductStatuses().ToArray();
            if (statusTypes.Any())
            {
                parameters.Add(new AnyFilter($"elements.{Brewer.ProductStatusCodename}", statusTypes));
            }

            var response = await _client.GetItemsAsync<Brewer>(parameters);

            return PartialView("ProductListing", response.Items);
        }

        private IList<SelectListItem> GetTaxonomiesAsSelectList(TaxonomyGroup taxonomyGroup)
        {
            return taxonomyGroup.Terms.Select(x => new SelectListItem { Text = x.Name, Value = x.Codename }).ToList();
        }
    }
}
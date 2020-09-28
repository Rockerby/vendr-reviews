﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Vendr.Core.Web.Api;
using Vendr.Contrib.ProductReviews.Models;
using Vendr.Contrib.ProductReviews.Services;
using Umbraco.Core.Services;
using Umbraco.Web.Models;
using Umbraco.Web.Models.ContentEditing;
using Notification = Umbraco.Web.Models.ContentEditing.Notification;
using Vendr.Contrib.ProductReviews.Enums;
using Vendr.Contrib.ProductReviews.Web.Dtos;
using Vendr.Core.Adapters;

namespace Vendr.Contrib.ProductReviews.Web.Controllers
{
    [PluginController("VendrProductReviews")]
    public class ProductReviewApiController : UmbracoAuthorizedApiController
    {
        //private readonly IVendrApi _vendrApi;
        private readonly IProductReviewService _productReviewService;
        private readonly ILocalizedTextService _textService;
        private readonly IProductAdapter _productAdapter;

        public ProductReviewApiController(
            IProductReviewService productReviewService,
            ILocalizedTextService textService,
            IProductAdapter productAdapter)
        {
            _productReviewService = productReviewService;
            _textService = textService;
            _productAdapter = productAdapter;
        }

        [HttpGet]
        public Dictionary<string, string> GetProductData(string productReference, string languageIsoCode)
        {
            var snapshot = _productAdapter.GetProductSnapshot(productReference, languageIsoCode);
            if (snapshot == null)
                return null;

            return new Dictionary<string, string>
            {
                { "storeId", snapshot.StoreId.ToString() },
                { "sku", snapshot.Sku },
                { "name", snapshot.Name }
            };
        }

        [HttpGet]
        public ProductReview GetProductReview(Guid id)
        {
            return _productReviewService.GetProductReview(id);
        }

        [HttpGet]
        public IEnumerable<ProductReview> GetProductReview(Guid[] ids)
        {
            return _productReviewService.GetProductReviews(ids);
        }

        [HttpGet]
        public PagedResult<ProductReview> GetProductReviews(Guid storeId, string productReference, long pageNumber = 1, int pageSize = 50)
        {
            long total;
            var items = _productReviewService.GetProductReviews(storeId, productReference, pageNumber, pageSize, out total);

            return new PagedResult<ProductReview>(total, pageNumber, pageSize)
            {
                Items = items
            };
        }

        [HttpGet]
        public PagedResult<ProductReview> GetProductReviewsForCustomer(Guid storeId, string customerReference, long pageNumber = 1, int pageSize = 50)
        {
            long total;
            var items = _productReviewService.GetProductReviewsForCustomer(storeId, customerReference, pageNumber, pageSize, out total);

            return new PagedResult<ProductReview>(total, pageNumber, pageSize)
            {
                Items = items
            };
        }

        [HttpGet]
        public PagedResult<ProductReview> SearchProductReviews(Guid storeId, long pageNumber = 1, int pageSize = 50, [FromUri] string[] statuses = null, [FromUri] decimal[] ratings = null, string searchTerm = null)
        {
            long total;
            var items = _productReviewService.SearchProductReviews(storeId, pageNumber, pageSize, out total, statuses: statuses, ratings: ratings, searchTerm: searchTerm);

            return new PagedResult<ProductReview>(total, pageNumber, pageSize)
            {
                Items = items
            };
        }

        [HttpPost]
        public ProductReview SaveReview(ProductReview review)
        {
            review.Notifications.Clear();

            try
            {
                var result = _productReviewService.SaveProductReview(review);
                result.Notifications.Add(new Notification(
                        _textService.Localize("speechBubbles/operationSavedHeader"),
                        string.Empty,
                        NotificationStyle.Success));
            }
            catch
            {
                review.Notifications.Add(new Notification(
                        _textService.Localize("speechBubbles/operationFailedHeader"),
                        string.Empty,
                        NotificationStyle.Error));
            }

            return review;
        }

        [HttpDelete]
        [HttpPost]
        public void DeleteReview(Guid id)
        {
            _productReviewService.DeleteProductReview(id);
        }

        [HttpPost]
        public ProductReview ChangeReviewStatus(StatusDto model)
        {
            return _productReviewService.ChangeStatus(model.ReviewId, model.Status);
        }
    }
}
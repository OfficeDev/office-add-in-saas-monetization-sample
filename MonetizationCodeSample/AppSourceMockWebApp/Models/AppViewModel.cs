// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AppSourceMockWebApp.Models
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.AspNetCore.Mvc.Rendering;
    using SaaSOfferMockData;

    public class AppViewModel
    {
        public Offer AppOffer { get; set; }

        public Plan Plan { get; set; }

        public int? Quantity { get; set; }

        public IEnumerable<SelectListItem> PlanList
        {
            get
            {
                return Service.MockDataService.Plans.Select(plan => new SelectListItem()
                {
                    Text = plan.PlanId,
                    Value = plan.PlanId
                });
            }
        }
    }
}

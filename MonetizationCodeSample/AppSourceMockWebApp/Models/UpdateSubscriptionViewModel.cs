// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AppSourceMockWebApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.AspNetCore.Mvc.Rendering;

    public class UpdateSubscriptionViewModel
    {
        public Subscription Subscription { get; set; }

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

        public Guid? OperationUpdateId { get; set; }
    }
}

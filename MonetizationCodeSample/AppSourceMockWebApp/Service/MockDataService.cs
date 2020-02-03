// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AppSourceMockWebApp.Service
{
    using System.Collections.Generic;
    using AppSourceMockWebApp.Models;

    public class MockDataService
    {
        public static List<Plan> Plans;

        static MockDataService()
        {
            Plans = new List<Plan>()
            {
                new Plan()
                {
                     PlanId = "SeatBasedPlan"
                },
                new Plan
                {
                    PlanId = "SiteBasedPlan"
                }
            };
        }
    }
}

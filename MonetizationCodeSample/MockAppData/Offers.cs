// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSOfferMockData
{
    public class Offers
    {
        public static Offer ContosoAppOffer
        {
            get
            {
                return new Offer()
                {
                    OfferID = "contoso_o365_addin",
                    Name = "Contoso Apps"
                };
            }
        }
    }

    // please go to the below link to learn more about SaaS offer creation
    // https://docs.microsoft.com/en-us/azure/marketplace/partner-center-portal/offer-creation-checklist

    public class Offer
    {
        public string OfferID { get; set; }

        public string Name { get; set; }

        /* Just for demonstration, so omit unnecessary properties */
        /* ... */
    }
}

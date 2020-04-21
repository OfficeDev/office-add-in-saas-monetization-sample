// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace ExcelAddInWeb.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}

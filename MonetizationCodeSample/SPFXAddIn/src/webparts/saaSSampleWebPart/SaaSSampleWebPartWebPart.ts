// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Version } from '@microsoft/sp-core-library';
import {
  BaseClientSideWebPart,
  IPropertyPaneConfiguration,
  PropertyPaneTextField
} from '@microsoft/sp-webpart-base';
import { escape } from '@microsoft/sp-lodash-subset';
import { AadHttpClient, HttpClientResponse, IHttpClientOptions } from '@microsoft/sp-http';
import styles from './SaaSSampleWebPartWebPart.module.scss';
import * as strings from 'SaaSSampleWebPartWebPartStrings';

const logoImage: string = require('./assets/logo.png');

export interface ISaaSSampleWebPartWebPartProps {
  description: string;
  clientId: string;
  sassWebApi: string;
}

export default class SaaSSampleWebPartWebPart extends BaseClientSideWebPart<ISaaSSampleWebPartWebPartProps> {
  private saasClient: AadHttpClient;
  private graphClient: AadHttpClient;

  protected onInit(): Promise<void> {
    return new Promise<void>((resolve: () => void, reject: (error: any) => void): void => {
      this.context.aadHttpClientFactory
        .getClient('https://graph.microsoft.com')
        .then((graphClient: AadHttpClient) => {
          this.graphClient = graphClient;
          this.context.aadHttpClientFactory
            .getClient(this.properties.clientId)
            .then((client: AadHttpClient): void => {
              this.saasClient = client;
              resolve();
            }, err => reject(err));
        }, err => reject(err));
    });
  }

  public render(): void {
    
    this.context.statusRenderer.displayLoadingIndicator(this.domElement, 'querying');

    var requestHeaders: Headers = new Headers();
    requestHeaders.append('Content-type', 'application/json');

    var requestOptions: IHttpClientOptions = {
      headers: requestHeaders,
      body: ""
    };
    this.graphClient
      .get('https://graph.microsoft.com/v1.0/me', AadHttpClient.configurations.v1)
      .then(response => {
        return response.json();
      })
      .then((user: any): void => {
        var welcome = `Welcome ${user.mail}`;
        this.saasClient
          .post(this.properties.sassWebApi, AadHttpClient.configurations.v1, requestOptions)
          .then((res: HttpClientResponse): Promise<any> => {
            return res.json();
          })
          .then((activation: any): void => {
            var result = activation.status == "Success" ? "You do have a paid license " : "You don't have a paid license ";
            console.log(`DEBUG: You have ${activation.availableLicenseQuantity == null ? "0" : activation.availableLicenseQuantity}  licenses available in your tenant`);
            console.log(`DEBUG: ${activation.reason}`);
            console.log(`DEBUG: Overrun is ${activation.allowOverAssignment === false ? "disabled" : "enabled"}`);
            console.log(`DEBUG: Auto-license-assignment is ${activation.firstComeFirstServedAssignment === false ? "disabled" : "enabled"}`);

            this.context.statusRenderer.clearLoadingIndicator(this.domElement);
            this.domElement.innerHTML = `
              <div class="${ styles.saaSSampleWebPart}">
                <div class="${ styles.container}">
                <div class="${ styles.row}">
                    <div class="${ styles.logo}">
                      <span><img src="${logoImage}" alt='an image' /></span><span>SaaS Sample SPFx add-in</span>
                    </div>
                  </div>
                  <div class="${ styles.row}">
                    <h2>${welcome}</h2>
                  </div>
                  <div class="${ styles.row}">
                    <span>${result}</span>
                  </div>
                </div>
              </div>`;
          }, (err: any): void => {
            this.context.statusRenderer.renderError(this.domElement, err);
          });
      }, (err: any): void => {
        this.context.statusRenderer.renderError(this.domElement, err);
      });
  }


  protected get dataVersion(): Version {
    return Version.parse('1.0');
  }

  protected getPropertyPaneConfiguration(): IPropertyPaneConfiguration {
    return {
      pages: [
        {
          header: {
            description: strings.PropertyPaneDescription
          },
          groups: [
            {
              groupName: strings.BasicGroupName,
              groupFields: [
                PropertyPaneTextField('description', {
                  label: strings.DescriptionFieldLabel
                }),
                PropertyPaneTextField('clientId', {
                  label: 'Client Id Text Field',
                  multiline: false
                }),
                PropertyPaneTextField('sassWebApi', {
                  label: 'Saas Web Api Text Field',
                  multiline: false
                })
              ]
            }
          ]
        }
      ]
    };
  }
}

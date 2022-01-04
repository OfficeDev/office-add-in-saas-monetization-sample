import { ComponentDialog, WaterfallDialog, WaterfallStepContext, OAuthPrompt, ConfirmPrompt, DialogTurnResult, DialogSet, DialogTurnStatus } from "botbuilder-dialogs";
import { TokenResponse, MessageFactory, TurnContext, StatePropertyAccessor } from "botbuilder";
import { LogoutDialog } from "./LogoutDialog";

const WATERFALL_DIALOG = 'WATERFALL_DIALOG';

export class MainDialog extends LogoutDialog {
    constructor(dialogId: string, connectionName: string){
        super(dialogId, connectionName);

        this.addDialog(new OAuthPrompt("OAuthPrompt",
         {
             connectionName: connectionName,
             text: "Please login",
             title: "Login",
             timeout: 300000
        })).addDialog(new ConfirmPrompt("ConfirmPrompt"))
        .addDialog(new WaterfallDialog(WATERFALL_DIALOG, [
            this.PromptStepAsync.bind(this),
            this.LoginStepAsync.bind(this)
        ]));

        this.initialDialogId = WATERFALL_DIALOG;
    }

    public async run(turnContext: TurnContext, accessor: StatePropertyAccessor) {
        const dialogSet = new DialogSet(accessor);
        dialogSet.add(this);

        const dialogContext = await dialogSet.createContext(turnContext);
        const results = await dialogContext.continueDialog();
        if (results.status === DialogTurnStatus.empty) {
            await dialogContext.beginDialog(this.id);
        }
    }

    private async PromptStepAsync(stepContext: WaterfallStepContext) : Promise<DialogTurnResult> {
        return await stepContext.beginDialog("OAuthPrompt");
    }

    private async LoginStepAsync(stepContext: WaterfallStepContext) : Promise<DialogTurnResult> {
        let tokenResponse = stepContext.result as TokenResponse;
        if (tokenResponse.token != null) {
            const apiEndpoint: string = process.env.SAAS_API as string;
            let requestHeaders: any = {
                'Content-Type': 'application/json',
                'Authorization': `bearer ${tokenResponse.token}`
            };

            const response = await fetch(apiEndpoint, {
                method: 'POST', 
                headers: requestHeaders
            });

            const activation: any = await response.json();

            await stepContext.context.sendActivity(activation.status === "Failure" ? "You don't have a paid license " : "You do have a paid license ");
            await stepContext.context.sendActivity(`DEBUG: You have ${activation.availableLicenseQuantity === null ? "0" : activation.availableLicenseQuantity} licenses available in your tenant`);
            await stepContext.context.sendActivity(`DEBUG: ${activation.reason}`);
            await stepContext.context.sendActivity(`DEBUG: Overrun is ${activation.allowOverAssignment === false ? "disabled" : "enabled"}`);
            await stepContext.context.sendActivity(`DEBUG: Auto-license-assignment is ${activation.firstComeFirstServedAssignment === true ? "enabled" : "disabled"}`);

            return await stepContext.endDialog();
        }
        
        await stepContext.context.sendActivity(MessageFactory.text("Login was not successful please try again."))
        return await stepContext.endDialog();
    }
}
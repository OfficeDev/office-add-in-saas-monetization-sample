import { ComponentDialog, DialogContext, DialogTurnResult } from "botbuilder-dialogs";
import { ActivityTypes, BotFrameworkAdapter } from "botbuilder";

export class LogoutDialog extends ComponentDialog{
    protected ConnectionName : string;

    constructor(id: string, connectionName: string)
    {
        super(id);
        this.ConnectionName = connectionName
    }

    async onBeginDialog(innerDC: DialogContext, options?: {} | undefined): Promise<DialogTurnResult>{
        const result = await this.interrupt(innerDC);
        if (result){
            return result;
        }
        return await super.onBeginDialog(innerDC);
    }

    async onContinueDialog(innerDC: DialogContext, options?: {} | undefined): Promise<DialogTurnResult>{
        const result = await this.interrupt(innerDC);
        if (result) {
            return result;
        }

        return await super.onContinueDialog(innerDC);
    }

    async interrupt(innerDC: DialogContext): Promise<DialogTurnResult|null> {
        if (innerDC.context.activity.type === ActivityTypes.Message) {
            const text = innerDC.context.activity.text ? innerDC.context.activity.text.toLowerCase() : '';
            if (text === 'logout') {
                // The bot adapter encapsulates the authentication processes.
                const botAdapter = innerDC.context.adapter as BotFrameworkAdapter;
                await botAdapter.signOutUser(innerDC.context, process.env.ConnectionName);
                await innerDC.context.sendActivity('You have been signed out.');
                return await innerDC.cancelAllDialogs();
            }
        }
        return null;
    }
}
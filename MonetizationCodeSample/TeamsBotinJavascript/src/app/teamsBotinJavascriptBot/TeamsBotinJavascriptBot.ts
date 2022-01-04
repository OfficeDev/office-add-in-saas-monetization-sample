import {
  BotDeclaration,
  MessageExtensionDeclaration,
  PreventIframe,
} from "express-msteams-host";
import * as debug from "debug";
import { DialogSet, DialogState } from "botbuilder-dialogs";
import {
  StatePropertyAccessor,
  CardFactory,
  TurnContext,
  MemoryStorage,
  ConversationState,
  ActivityTypes,
  TeamsActivityHandler,
  UserState,
  SigninStateVerificationQuery
} from "botbuilder";
import HelpDialog from "./dialogs/HelpDialog";
import WelcomeCard from "./dialogs/WelcomeDialog";
import { MainDialog } from "./dialogs/MainDialog";

// Initialize debug logging module
const log = debug("msteams");

/**
 * Implementation for TeamsBotinJavascript Bot
 */
@BotDeclaration(
  "/api/messages",
  new MemoryStorage(),
  process.env.MICROSOFT_APP_ID,
  process.env.MICROSOFT_APP_PASSWORD
)
export class TeamsBotinJavascriptBot extends TeamsActivityHandler {
  private readonly conversationState: ConversationState;
  private readonly dialogs: DialogSet;
  private dialogState: StatePropertyAccessor<DialogState>;
  private readonly userState: UserState;
  private readonly mainDialog: MainDialog;

  /**
   * The constructor
   * @param conversationState
   */
  public constructor(conversationState: ConversationState) {
    super();

    this.conversationState = conversationState;
    this.dialogState = conversationState.createProperty("dialogState");
    this.dialogs = new DialogSet(this.dialogState);
    this.dialogs.add(new HelpDialog("help"));
    this.userState = new UserState(new MemoryStorage());
    this.mainDialog = new MainDialog(
      "mainDialog",
      process.env.BOT_CONNECTION_NAME || ""
    );

    // Set up the Activity processing

    this.onMessage(
      async (context: TurnContext): Promise<void> => {
        // TODO: add your own bot logic in here
        switch (context.activity.type) {
          case ActivityTypes.Message:
            let text = TurnContext.removeRecipientMention(context.activity);
            text = text.toLowerCase();
            if (text.startsWith("hello")) {
              await context.sendActivity("Oh, hello to you as well!");
              return;
            } else if (text.startsWith("help")) {
              const dc = await this.dialogs.createContext(context);
              await dc.beginDialog("help");
            } else {
              await this.mainDialog.run(context, this.dialogState);
            }
            break;
          default:
            break;
        }
        // Save state changes
        return this.conversationState.saveChanges(context);
      }
    );

    this.onConversationUpdate(
      async (context: TurnContext): Promise<void> => {
        if (
          context.activity.membersAdded &&
          context.activity.membersAdded.length !== 0
        ) {
          for (const idx in context.activity.membersAdded) {
            if (
              context.activity.membersAdded[idx].id ===
              context.activity.recipient.id
            ) {
              const welcomeCard = CardFactory.adaptiveCard(WelcomeCard);
              await context.sendActivity({ attachments: [welcomeCard] });
            }
          }
        }
      }
    );

    this.onMessageReaction(
      async (context: TurnContext): Promise<void> => {
        const added = context.activity.reactionsAdded;
        if (added && added[0]) {
          await context.sendActivity({
            textFormat: "xml",
            text: `That was an interesting reaction (<b>${added[0].type}</b>)`,
          });
        }
      }
    );
  }

  async onTurnActivity(context: TurnContext): Promise<void> {
    console.log("on Turn Activity");
    await super.onTurnActivity(context);

    await this.conversationState.saveChanges(context, false);
    await this.userState.saveChanges(context, false);
  }

  async handleTeamsSigninVerifyState(
    context: TurnContext,
    query: SigninStateVerificationQuery
  ): Promise<void> {
    console.log("handleTeamsSigninVerifyState");
    await this.mainDialog.run(context, this.dialogState);
  }
}

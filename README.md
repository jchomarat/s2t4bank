# S2TforBank

This bot illustrate how to use LUIS in multiple languages based on client locales. It does use as well Custom Speech recognition. This bot recognizes a typical bank scenario where a user ask for a loan with a specific amount for a timeframe and with an insurance. A typical sentence to dictate or write would be "I want a 100000 euros loan for a 15 years period without insurance". 

So far English and French has been implemented as an example. In French, a sentence like "Je veux faire un emprunt de 100000 euros pendant 15 ans avec assurance décès" will work perfectely.

This bot has been created using [Bot Framework](https://dev.botframework.com), it shows how to:

- Use [LUIS](https://www.luis.ai) to implement core AI capabilities in multiple languages
- Implement a multi-turn conversation using Dialogs
- Handle user interruptions for such things as `Help` or `Cancel` in multiple languages
- Prompt for and validate requests for information from the user

Topics from this file:
- [Prerequisites](#Prerequisites)
  - [Create a LUIS application](#Create-a-luis-application-to-enable-language-understanding) to enable language understanding
    - [Adding keys and App Id](#Adding-keys-and-app-id-to-the-appsettings.json-file) to the appsettings.json file
    - [Entities](#Entities)
    - [Importing LUIS models](#Importing-luis-models)

## Prerequisites

This sample **requires** prerequisites in order to run.

### Create a LUIS application to enable language understanding

This bot uses [LUIS](https://www.luis.ai), an AI based cognitive service, to implement language understanding. 

#### Importing LUIS models


To create and import the LUIS models. You can create multiple LUIS by language importing the models you'll find in the [LUISModels directory](/LUISModels). You will fond French and English models.

1. Create a new App
2. Select French for a French model and English for the English one. 

![luis language](/docs/luislanguage.png)

3. Go to *Manage* on top menu then *Version* on left menu then *Import Version*. Select the version associated to the language you have created.

![luis import](/docs/luisimport.png)

4. Train
5. Publish

You can now access to the keys to correctly reach the end point.

#### Entities

The model is simple using 3 entities:
- the built in Datetime.v2 entity to recognize the periods
- the built in Money entity to recognize the amounts
- a list for insurance

![luis insurance](/docs/luisinsurance.png)

Note that the list is made from synonyms. The sub entity can be anything. As this sample started in French, we made them matching the possible insurance entities. For simplicity reasons, the code only uses one type of insurance. LUIS will gives you all the possible ones.

#### Adding keys and App Id to the appsettings.json file

Once imported and trained, put the keys in the correction section of the ```appsettings.json``` file.

The entries will look like that:

```json
"LuisAppId-fr-FR": "1e4327d9-abcd-1234-9c3f-0663c027aa22",
"LuisAPIKey-fr-FR": "4da74321778abcfed5e261234522bb1a",
"LuisAPIHostName-fr-FR": "westus.api.cognitive.microsoft.com",
"LuisAppId-en-US": "d0e58ef5-4321-abcd-ef12-012342dc6e3b",
"LuisAPIKey-en-US": "4daabcd123474b1234abcdbb0123bb1a",
"LuisAPIHostName-en-US": "westus.api.cognitive.microsoft.com",
```

To add new languages, just add new keys with the locale you are targeting. In this example eu-US and fr-FR. Note that every LUIS can be located on a different region allowing you to reduce latency close by where your customers are.

### Install .NET Core CLI

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 2.1 or higher.

  ```bash
  # determine dotnet version
  dotnet --version
  ```

- If you don't have an Azure subscription, create a [free account](https://azure.microsoft.com/free/).
- Install the latest version of the [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli?view=azure-cli-latest) tool. Version 2.0.54 or higher.

## To try this sample

- In a terminal, navigate to `S2T4Bank`

    ```bash
    # change into project folder
    cd S2T4Bank
    ```

- Run the bot from a terminal or from Visual Studio, choose option A or B.

  A) From a terminal

  ```bash
  # run the bot
  dotnet run
  ```

  B) Or from Visual Studio

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `S2Tf4Bank` folder
  - Select `S2T4Bank.csproj` file
  - Press `F5` to run the project

## Testing the bot using Bot Framework Emulator

[Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the Bot Framework Emulator version 4.3.0 or greater from [here](https://github.com/Microsoft/BotFramework-Emulator/releases)

Don't forget to create your application key and secret and place them in the ```appsettings.json``` file. The entry will look like that:

```json
"MicrosoftAppId": "63fc1234-4321-abcd-0123-19a12346e51d",
"MicrosoftAppPassword": "0123456|789+c_abcdef**ocF_F**ck/",
```

### Connect to the bot using Bot Framework Emulator

- Launch Bot Framework Emulator
- File -> Open Bot from ```s2t.bot```. Don't forget to add your App Id and App Secret into the file.
- Enter a Bot URL of `http://localhost:3978/api/messages`

## Use the bot with the WebChat client in order to use the microphone

### Simplify the way to train your model

Our colleagues have built this [pipeline](https://github.com/msimecek/Speech-Training-Pipeline) to simplify the process of preparing and training a speech to text (STT) models for the Speech Service, which is part of Microsoft Azure.

### Setup the bot

This bot also include the [WebChat client](https://github.com/Microsoft/BotFramework-WebChat), tweaked, in order to hook a custom speech model.
To wrap up everything, don't forget to add the following parameters to the ```appsettings.json```:

```json
"Region": "Speech service region, ex: westeurope",
"SpeechKey": "Speech service key",
"SpeechEndPointId-fr-FR": "Speech model endpoint ID for french",
"SpeechEndPointId-en-US": "Speech model endpoint ID for english",
"DirectlineToken": "Token for the directline access of the WebChat client to your bot"
```

Please note that for the speech model endpoint ID, you have 3 of them: one for up to 15 seconds speech, one for up to 10 minutes and one for up to 10 minutes with support of dictation of punctuation marks. Use the second one (15 seconds is quite short)

Also note that to get a directline token, you need to connect to the [Azure portal](https://portal.azure.com) and add the Channel "Direct Line". You will then be able to retreive the token.

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Dialogs](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-dialog?view=azure-bot-service-4.0)
- [Gathering Input Using Prompts](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-prompts?view=azure-bot-service-4.0&tabs=csharp)
- [Activity processing](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-activity-processing?view=azure-bot-service-4.0)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Azure Bot Service Documentation](https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0)
- [.NET Core CLI tools](https://docs.microsoft.com/en-us/dotnet/core/tools/?tabs=netcore2x)
- [Azure CLI](https://docs.microsoft.com/cli/azure/?view=azure-cli-latest)
- [Azure Portal](https://portal.azure.com)
- [Language Understanding using LUIS](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/)
- [Channels and Bot Connector Service](https://docs.microsoft.com/en-us/azure/bot-service/bot-concepts?view=azure-bot-service-4.0)



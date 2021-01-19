# Demo erp connector application

**Tested in Litium version: [Litium 7.4]**

ERP Connect integration application would connect to a Litium server instance, through web api registration. 

Follow the guide here to setup the Integration application : https://docs.litium.com/documentation/litium-connect/develop-integration-applications

How to use Postman collection samples : This is a guide to import sample requests into postman : https://docs.litium.com/documentation/litium-documentation/sales/return-management_1_1/api-testing

## Instructions

This is the guide for building a sample ERP connector integration application.

ERP Connect integration application would connect to a Litium server instance, through web api registration. The process is described below.

1. Download the sample integration application (require login to docs site with download permissions)
ERP Connector Sample demo integration application and sample postman collection could be download in Litium github here >>. 

This sample integration application would be running its own life-cycle as a seperate web application. Also please note that the samples in litium github are not maintained and updated when new versions of Litium are released.

2. Configure endpoints 

In web.config we could config the endpoint for Litium Connect server, here is an example appSettings section in the demo application to connect to Litium Connect server: 

```
<appSettings>
    <add key="MS_WebHookReceiverSecret_Litium" value="<Secrect value>" />
    <add key="WebHookSelfRegistrationCallbackHost" value="<Application call back url>"/>
    <add key="WebHookSelfRegistrationHost" value="<Litium Server instance>"/>
    <add key="WebHookSelfRegistrationClientId" value="<ServiceAccount username>"/>
    <add key="WebHookSelfRegistrationClientSecret" value="<ServiceAccount password>"/>
</appSettings>
```
  
  
 - MS_WebHookReceiverSecrect_Lithium: Secrect code that will be sent over to litium Connect server. This secrect code will be included in the header of the server's notification back to this connector. Request this code from Litium Support.
  
  - WebHookSelfRegistrationCallbackHost: Hosting url of this ERP sample demo application. Litium Connect server will send a notification to this address when the subscribed event was raised. This url should be visible to the Litium server, the one you configure in WebHookSelfRegistrationHost appSetting. During auto registration, this url will be sent to the Litium server.
  
  - WebHookSelfRegistrationHost: Url of Litium server. During self registration flow we need to create an registration and send to this address to subscribe to a event on server.
  
  - WebHookSelfRegistrationClientId: Id of Service Account for accessing Litium API.
  
  - WebHookSelfRegistrationClientSecrect: Secrect code / Password of Service Account to access Litium API. 

3. Register Erp Connector Application to Litium server

Update the Litium.Connect.Erp.Abstractions package to latest release (pre-release package when in Beta)
Build the application and host it in IIS.
ERP Connector application sample has a build in auto registration (file name Global.asax). These code will be executed at startup and will try to subscribe to OrderConfirmed event for a given host using the info we added in step #1.

Optional: One may manually register/ double check the webhooks registration using web api endpoints as described here >>.

Any error that happens during the seft-registration process will be logged in src\ErpDemo.log file.

4. Litium Receiver

Litium Connect server uses Asp.net Webhook (more info here) to broacast messages when an given event is triggered. Erp connector application is acting as a webhook recever so we need to follow the same pattern to receive and parse the message. 

Litium Receiver implement WebHookReceiver and has ReceiveAsync as the main processing method. If the incoming message to receiver is an Get request , it should be an webhook Verification request and the receiver must echo the message back to confirm what was just received. If the incoming message was a Post request then that was an webhook update from server, the code will trigger Litium Handler to process the message.

5. Litium Handler

Litium Handler is defined to process the message which was sent over from Connect server, currently we wrote all the raw data into log files that looks like this

6. OpenApi Specification Generation.

Litium use OpenAPI Specification to define the Litium Api so you can generate the client code easily.
You can go to Litium server to get the new specification files
Or you can use the existing one which are included to Litium.SampleApps.Erp project.
And then go to the Service References to add/update the options for generated code.
The generated code will be generated under obj folder
There's also another way to generate the client code by using NSwagStudio. More detail could be found here.

7. Testing Demo Erp Connector Application

The application could be tested by importing the sample query collection to Postman, which could be dowloaded here >>. 
For setting up postman, please see this article.
There are 12 steps in the postman collection, which will calling to application to do these steps:
    
    1: Retrieve the order, to run this step order externalId must be provided. We could try to book an order on Litium Connect for any two items with the quality = 2 each and copy the order number to Postman variable: 
    
    2: Create partial shipment: Create a partial shipment for the first two items of the order. Shipping quality = 1 for each item.
    
    3: Notify the first shipment delivered: Update the first shipment status to delivered. This will make Connect raise ReadyToShip event.
    
    4: Build Rma from return split: Start return process to return the first item in the delivered shipment. More info on sales return management process could be found here>>.
    
    5 to 7: Change Rma states all the way to Approved.
    
    8 and 9:  Retrieve and Confirm the Sales Return Order.
    
    10: Refund the money for the sales return order.
    
    11: Create a second shipment, ship another two items of the order.
    
    12: Notify the second shipment as delivered. If these was the last shipment of the order then the order state should be changed to Completed after this step.

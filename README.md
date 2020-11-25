# Litium Samples

## Important!

Read this before using code from this repository.

1. The samples are not supported by Litium
2. The samples are not maintained and updated when new versions of Litium are released
3. A sample may be created only as a demo and not for production use
4. The samples may not be tested thouroghly

However, with this in mind the samples can be used as inspiration or a starting point when implementing a specific solution in a project, just remember to test thouroghly before using the code in a production environment

## Samples

- [Block editorial section from Demo](Samples/Block%20editorial%20section%20from%20Demo) - Block used on public Litium demowebsite
- [Block hero banner from Demo](Samples/Block%20hero%20banner%20from%20Demo) - Block used on public Litium demowebsite
- [Custom field type YouTube video](Samples/Custom%20field%20type%20Youtube%20video)
- [Display recently visited products](Samples/Display%20recently%20visited%20products)
- [Logging with Stackexchange Exceptional (WebLog)](Samples/Logging%20with%20Stackexchange%20Exceptional%20%28WebLog%29) - Restore the WebLog that was removed in Litium version 7.4
- [Login with Auth0](Samples/Login%20with%20Auth0) - Use the external identity provider Auth0 to login to Litium
- [Sliding date span on smart groups](Samples/Sliding%20date%20span%20on%20smart%20groups) - Smart groups in Litium standard only support fixed date intervals
- [Target group condition Login status](Samples/Target%20group%20condition%20Login%20status) - Target group condition to filter out all anonymous or logged in users
- [Use WebForms custom panel in 7.4](Samples/Use%20legacy%20custom%20panel%20in%207.4) - Use WebForms custom panel in 7.4.

## External samples

Samples hosted by others outside this repo

- [Distancify.LitiumAddOns.MediaMapper](https://github.com/distancify/Distancify.LitiumAddOns.MediaMapper) by [Distancify](https://distancify.com/) - Programmatically organize media and link media to other entities
- [Litium.AddOns.GoogleMapFieldType](https://github.com/tonnguyen/litium-addons-googlemap) - Use Google Map to show and edit geo location
- [Litium.AddOns.SmartImage](https://github.com/tonnguyen/litium-addons-smartimage) - Analyzes and extracts rich information from images, powered by machine learning, to categorize images in a better way
- [Litium.AddOns.SmartImage.GoogleCloudVision](https://github.com/tonnguyen/litium-addons-smartimage-googlecloudvision) - Implementation of above using Google Cloud Vision
- [Litium.FieldType.Bag](https://github.com/tonnguyen/litium-fieldtype-bag) - Contains several custom Litium Field types 
- [LitiumDemoHelper](https://github.com/martenw/litium-demo-helper) - Useful tools when creating a customer demo
- [LitiumLinksField](https://github.com/martenw/LitiumLinksField) - Field definition for Litium PIM to show all links where a product/category is published
- [LitiumWebsiteTexts](https://github.com/martenw/LitiumWebsiteTexts) - Create and maintain website texts in code 

## How to add a new sample

If you have a sample hosted elsewhere just create a PR where you add a link to your sample in the _External samples_ list above.

If you want to submit a sample to be hosted in this repository please submit a PR for review, just follow the instructions below:

1. Copy the folder *"Samples/_Sample template"* and rename the copied folder to describe the sample
2. Edit the _README_ file in the folder to describe your sample
3. Add sample code in the Resources-folder and remove the Placeholder.txt file
# Use legacy custom panel in 7.4

Since [Litium 7.4](https://docs.litium.com/documentation/whats-new/what-s-new-7-4), the system no longer supports creating custom panels using WebForms. Custom panels are recommended to be implemeneted in either Angular, or as an external page and embed to the system by using its URL. More information of how to create custom panel can be found [here](https://docs.litium.com/documentation/architecture/back-office_1/creating-custom-panel).

It is possible to re-use your existing custom panels, written in WebForms, by wrapping the Web user control in an .aspx page, then embed its URL in a Panel Definition.

## Instructions

1. Assuming the Custom panel we have were defined as [SamplePanel.ascx](Resources/SamplePanel.ascx) and [SamplePanel.ascx.cs](Resources/SamplePanel.ascx.cs)

1. Create a [ASPX page](Resources/SamplePanelPage.aspx) to use that User control.

1. Define the Panel defition as described in the [guideline](https://docs.litium.com/documentation/architecture/back-office_1/creating-custom-panel), and set the Url field to the URL of SamplePanelPage.aspx page. A sample panel definition can be found [here](Resources/SamplePanelDefinition.cs).
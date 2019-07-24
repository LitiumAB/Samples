# Custom field type Youtube video

**Tested in Litium version: 7.2.3**

Custom field type for adding a YouTube video to any Litium entity. Enter the video id (https://www.youtube.com/watch?v=**XXXXXXXXXXX**) into the field. You can see and play the video if there's a value in the field.

![Screenshot of field in use in Litium PIM](Resources/Screenshot.png?raw=true)

[You can read more about creating your own custom field type on the Litium Docs site](https://docs.litium.com/documentation/architecture/field-framework/creating-a-custom-field-type)

## Instructions

1. Copy the files below to `\Src\Litium.Accelerator.FieldTypes` and include them in your project. 
    * `YoutubeFieldConstants.cs`
    * `YoutubeFieldMetaData.cs`
    * `YoutubeFieldTypeConverter.cs`
2. Copy the files below to `\Src\Litium.Accelerator.FieldTypes\src\Accelerator\components\field-editor-youtube`. 
    * `field-editor-youtube.component.html`
    * `field-editor-youtube.component.ts`
3. Add your new custom field to `\Src\Litium.Accelerator.FieldTypes\src\Accelerator\extension.ts`. An example is included in the Resources folder.
4. Build the client files.
5. When you want to display the video on a page, you need to wrap the embedded iframe in a container to make it responsive. Below is an example of how to achieve it. 

```html
<div class="youtube__content-container">
    <iframe class="youtube__content" 
        src="https://www.youtube.com/embed/<ID from ViewModel>" 
        frameborder="0" 
        allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture">
    </iframe>
</div>
```
```scss
.youtube {
    &__content-container {
        position: relative;
        width: 100%;
        height: 0;
        padding-bottom: 56.25%;
    }

    &__content {
        position: absolute;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
    }
}
```
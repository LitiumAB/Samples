import { Component, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { BaseFieldEditor } from 'litium-ui';
import { DomSanitizer } from '@angular/platform-browser';

@Component({
    selector: 'field-editor-youtube',
    templateUrl: './field-editor-youtube.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FieldEditorYoutube extends BaseFieldEditor {
    constructor(changeDetectorRef: ChangeDetectorRef, private _sanitizer: DomSanitizer) {
        super(changeDetectorRef);
    }

    getUrl(language: string): any {
        const value = super.getValue(language);
        const url = this._sanitizer.bypassSecurityTrustResourceUrl('https://www.youtube.com/embed/' + value);

        return url;
    }
}
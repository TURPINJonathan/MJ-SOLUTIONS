import { CommonModule } from '@angular/common';
import { Component, Input, TemplateRef, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { capitalizeFirstLetter } from '#shared/utils/string.utils';

@Component({
  selector: 'app-input',
  standalone: true,
  templateUrl: './input.html',
	imports: [CommonModule],
  styleUrl: './input.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => InputComponent),
      multi: true
    }
  ]
})
export class InputComponent implements ControlValueAccessor {
  @Input() type: string = 'text';
  @Input() placeholder: string = '';
  @Input() name: string = '';
  @Input() disabled: boolean = false;
  @Input() error: boolean = false;
  @Input() valid: boolean = false;
	@Input() label?: string;
	@Input() append?: string | TemplateRef<any>;
	@Input() prepend?: string | TemplateRef<any>;

  value: string = '';

  private onChange = (value: string) => {};
  private onTouched = () => {};

	get capitalizedLabel(): string | undefined {
    return this.label ? capitalizeFirstLetter(this.label) : undefined;
  }

  writeValue(value: string): void {
    this.value = value || '';
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  onInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.value = value;
    this.onChange(value);
  }

  onBlur(): void {
    this.onTouched();
  }

	isTemplate(value: any): value is TemplateRef<any> {
		return value instanceof TemplateRef;
	}
}
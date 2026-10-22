import { TestBed } from '@angular/core/testing';
import { RiskObjectValue } from '../forms/risk-object-form';
import { RiskObjectEditor } from './risk-object-editor';

describe('RiskObjectEditor (S:RISK-AssetTypes)', () => {
  async function render(primaryName = '', hasPrimary = primaryName.length > 0) {
    const fixture = TestBed.createComponent(RiskObjectEditor);
    fixture.componentRef.setInput('hasPrimary', hasPrimary);
    fixture.componentRef.setInput('primaryName', primaryName);
    await fixture.whenStable();
    const submitted: RiskObjectValue[] = [];
    fixture.componentInstance.submitted.subscribe((value) => submitted.push(value));
    const element = fixture.nativeElement as HTMLElement;
    const button = (text: string) =>
      [...element.querySelectorAll('button')].find((candidate) => candidate.textContent?.trim() === text);

    return { fixture, element, submitted, button };
  }

  it('submits a trimmed risk object marked as primary', async () => {
    const { fixture, element, submitted, button } = await render();
    const description = element.querySelector<HTMLInputElement>('input[formcontrolname="assetDescription"]');
    description!.value = '  Kitchen  ';
    description!.dispatchEvent(new Event('input'));

    button('Mark as primary')?.click();
    await fixture.whenStable();
    button('Add')?.click();

    expect(submitted).toEqual([
      {
        assetType: 'Property',
        assetDescription: 'Kitchen',
        damageDescription: '',
        assetReference: '',
        isPrimary: true,
      },
    ]);
  });

  it('does not submit without an asset description', async () => {
    const { submitted, button } = await render();

    button('Add')?.click();

    expect(submitted).toEqual([]);
  });

  it('disables the primary toggle and names the current primary', async () => {
    const { element, button } = await render('Warehouse office');

    expect(button('Mark as primary')?.disabled).toBe(true);
    expect(element.textContent).toContain('Warehouse office is already the primary risk object.');
  });

  it('disables the primary toggle when the current primary has no description', async () => {
    const { element, button } = await render('', true);

    expect(button('Mark as primary')?.disabled).toBe(true);
    expect(element.textContent).toContain('Another risk object is already the primary risk object.');
  });
});

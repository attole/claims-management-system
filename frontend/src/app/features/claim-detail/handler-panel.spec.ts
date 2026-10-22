import { TestBed } from '@angular/core/testing';
import { FormControl } from '@angular/forms';
import { EMPTY_ID } from '../../core/api/api-ids';
import { DEV_USERS } from '../../core/auth/dev-users';
import { HandlerPanel } from './handler-panel';

describe('HandlerPanel (S:UI-Detail-Header)', () => {
  async function render(handlerId: string = DEV_USERS[0].id) {
    const fixture = TestBed.createComponent(HandlerPanel);
    fixture.componentRef.setInput('handlerId', handlerId);
    fixture.componentRef.setInput('handlers', DEV_USERS);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;
    const button = () => element.querySelector('button') as HTMLButtonElement;
    const control = (fixture.componentInstance as unknown as { handler: FormControl<string> }).handler;
    const pick = async (id: string) => {
      control.setValue(id);
      control.markAsDirty();
      await fixture.whenStable();
    };

    return { fixture, element, button, control, pick };
  }

  it('shows who the claim is assigned to', async () => {
    const { element } = await render();

    expect(element.textContent).toContain('Hana Handler');
  });

  it('shows an unassigned claim as Unassigned', async () => {
    const { element } = await render(EMPTY_ID);

    expect(element.textContent).toContain('Unassigned');
  });

  it('keeps Assign off until another handler is picked', async () => {
    const { button, pick } = await render();
    const initial = button().disabled;

    await pick(DEV_USERS[1].id);
    const afterPick = button().disabled;
    await pick(DEV_USERS[0].id);

    expect([initial, afterPick, button().disabled]).toEqual([true, false, true]);
  });

  it('emits the picked handler on Assign', async () => {
    const { fixture, button, pick } = await render();
    const assigned: string[] = [];
    fixture.componentInstance.assigned.subscribe((id) => assigned.push(id));

    await pick(DEV_USERS[2].id);
    button().click();

    expect(assigned).toEqual([DEV_USERS[2].id]);
  });

  it('turns Assign off and locks the select while saving or on a frozen claim', async () => {
    const { fixture, button, control, pick } = await render();
    await pick(DEV_USERS[1].id);

    fixture.componentRef.setInput('isSaving', true);
    await fixture.whenStable();
    const whileSaving = button().disabled;
    fixture.componentRef.setInput('isSaving', false);
    fixture.componentRef.setInput('isEditable', false);
    await fixture.whenStable();

    expect([whileSaving, button().disabled, control.disabled]).toEqual([true, true, true]);
  });

  it('follows the saved handler after a reload, but keeps a pick in progress', async () => {
    const { fixture, control, pick } = await render();

    fixture.componentRef.setInput('handlerId', DEV_USERS[1].id);
    await fixture.whenStable();
    const followed = control.value;
    await pick(DEV_USERS[2].id);
    fixture.componentRef.setInput('handlerId', DEV_USERS[0].id);
    await fixture.whenStable();

    expect([followed, control.value]).toEqual([DEV_USERS[1].id, DEV_USERS[2].id]);
  });
});

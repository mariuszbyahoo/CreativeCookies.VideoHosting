import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BlobsListComponent } from './blobs-list.component';

describe('BlobsListComponent', () => {
  let component: BlobsListComponent;
  let fixture: ComponentFixture<BlobsListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ BlobsListComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BlobsListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

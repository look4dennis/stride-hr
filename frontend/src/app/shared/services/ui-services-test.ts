// Simple test file to verify all UI services compile correctly
import { UIElementValidatorService } from './ui-element-validator.service';
import { ButtonHandlerService } from './button-handler.service';
import { DropdownDataService } from './dropdown-data.service';
import { SearchService } from './search.service';
import { FormValidationService } from './form-validation.service';
import { CRUDOperationsService } from './crud-operations.service';
import { UIIntegrationService } from './ui-integration.service';

// This file exists only to verify TypeScript compilation
// It should not be used in the actual application

export const UI_SERVICES = {
  UIElementValidatorService,
  ButtonHandlerService,
  DropdownDataService,
  SearchService,
  FormValidationService,
  CRUDOperationsService,
  UIIntegrationService
};
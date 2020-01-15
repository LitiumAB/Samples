# Target group condition: Login status

**Tested in Litium version: 7.3.1**

This target group condition makes it possible to filter target group members by their login status, either include all anonymous or non-anonymous users.

## Instructions

1. Copy content of the folder _Resources/LoginStatus_ to the Accelerator project folder _Src\Litium.Accelerator\TargetGroupconditions\LoginStatus_

1. Copy content of the folder _Resources/Translations_ to the Accelerator MVC site folder _Src\Litium.Accelerator.Mvc\Site\Resources\Administration_

1. The _TargetGroupFilterConditionModel_ class in the AutoMapper registration in _LoginStatusSetup.cs_ require a nuget reference to `Litium.Web.Administration.Application`, so add that reference to the _Litium.Accelerator_-project.

1. After a recompile the condition **Login status** should now be available when editing conditions of a target group in Litium backoffice.
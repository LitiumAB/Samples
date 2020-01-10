# Target group condition: Login status

**Tested in Litium version: 7.3.1**

This target group condition makes it possible to filter target group members by their login status, either include all anonymous or non-anonymous users.

## Instructions

1. Add all files in the _Resources_-folder to the Accelerator project in folder _Src\Litium.Accelerator\TargetGroupconditions\LoginStatus_

1. The _TargetGroupFilterConditionModel_ class in the AutoMapper registration in _LoginStatusSetup.cs_ require a nuget reference to `Litium.Web.Administration.Application`, so add that reference to the _Litium.Accelerator_-project.

1. Add the following strings to the file _\Src\Litium.Accelerator.Mvc\Site\Resources\Administration\Administration.resx_:

    |Key|Value|
    |--|--|
    | targetgroup.filter.loginstatus | Login status |
    | targetgroup.filter.loginstatus.anonusers | Include anonymous users |
    | targetgroup.filter.loginstatus.loginusers | Include logged in users |

1. Add the following strings to the file _\Src\Litium.Accelerator.Mvc\Site\Resources\Administration\Administration.sv-se.resx_:

    |Key|Value|
    |--|--|
    | targetgroup.filter.loginstatus | Inloggad status |
    | targetgroup.filter.loginstatus.anonusers | Inkludera anonyma användare |
    | targetgroup.filter.loginstatus.loginusers | Inkludera inloggade användare |

1. After a recompile the condition **Login status** should now be avaliable when editing conditions of a target group in Litium backoffice.
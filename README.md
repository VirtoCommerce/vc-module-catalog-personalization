# Overview

The main concept of VC Personalization module is to personalize the issuing of catalog, price lists and marketing actions using Tags and User Groups.

The Contacts should be first included into a specific User Groups and then the catalogs, price lists or promotions should be assigned to this UG.

## Key features

1. Control the visibility of catalog objects such as Product and Categories, through manual tagging of these objects with special property `Tags` which can contain multiple predefined values which also may be defined for customer profile
1. Inheritance `Tags` between catalogs objects depend on catalog taxonomy;
1. Allow using `User groups` from customer profile to products or categories filtration in the storefront search
1. Different policies for `Tags` propagation in catalog objects hierarchy:
    1. `UpTree` propagate the tags from descendants to parents up the hierarchy
    ![image](https://user-images.githubusercontent.com/7566324/62931481-ba630c00-bdbe-11e9-9cdf-6d05e955721b.png);
    1. `DownTree` inherits the tags by descendants from their parent down the hierarchy
    ![image](https://user-images.githubusercontent.com/7566324/62931421-a3241e80-bdbe-11e9-8f02-fd22d0fbcc6f.png).

## Documentation

1. [Catalog Personalization Document](/docs/index.md)
1. [View on Github](https://github.com/VirtoCommerce/vc-module-catalog-personalization/tree/dev)

## Installation

Installing the module:

1. Automatically: in VC Manager go to More -> Modules -> Catalog personalization module -> Install
1. Manually: download module zip package from https://github.com/VirtoCommerce/vc-module-catalog-personalization/releases. In VC Manager go to More -> Modules -> Advanced -> upload module package -> Install.

## References 

1. Deploy: https://virtocommerce.com/docs/latest/developer-guide/deploy-module-from-source-code/
1. Installation: https://www.virtocommerce.com/docs/latest/user-guide/modules/
1. Home: https://virtocommerce.com
1. Community: https://www.virtocommerce.org
1. [Download Latest Release](https://github.com/VirtoCommerce/vc-module-catalog-personalization/releases/)
## License
Copyright (c) Virto Solutions LTD.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

http://virtocommerce.com/opensourcelicense

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

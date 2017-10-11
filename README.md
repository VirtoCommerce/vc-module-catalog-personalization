# VirtoCommerce.CatalogPersonalization
VirtoCommerce.CatalogPersonalization module adds personalisation feature to the products catalog so that store owner now could define different user experiences for one catalog categories and products.

Key features:
* control the visibility of catalog objects such as Product and Categories, through manual tagging of these objects with special property `Tags` which can contains multiple predefined values which also may be defined for customer profile
* inheritance `Tags` between catalogs objects depend on catalog taxonomy (categories inherit this property from parent catalog or category, products inherit this property from parent category)
* allow to use `UserGroups` from customer profile to products or categories filtration in the storefront search

Managing catalog tags:
![catalog-tags](https://user-images.githubusercontent.com/7536694/31444159-ab3ba8ce-ae9b-11e7-9705-313c93784508.gif)

Managing category and product tags:
![category-product-tags](https://user-images.githubusercontent.com/7536694/31444403-7773b738-ae9c-11e7-807e-b6e9aca7cddb.gif)

Managing user groups:
![user-groups](https://user-images.githubusercontent.com/7536694/31445959-9fa693e4-ae9f-11e7-8568-b1ff7ff32da1.gif)

# Installation
Installing the module:
* Automatically: in VC Manager go to Configuration -> Modules -> Catalog personalization module -> Install
* Manually: download module zip package from https://github.com/VirtoCommerce/vc-module-catalog-personalization/releases. In VC Manager go to Configuration -> Modules -> Advanced -> upload module package -> Install.

# License
Copyright (c) Virtosoftware Ltd.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

http://virtocommerce.com/opensourcelicense

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

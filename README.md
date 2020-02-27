# VirtoCommerce.CatalogPersonalization

The main concept of VC Personalization module is to personalize the issuing of catalog, price lists and marketing actions using Tags and User Groups.

The users should be first included into  specific User Groups and then assign  catalogs, price lists or promotions to this UG.

## Key features

1. Control the visibility of catalog objects such as Product and Categories, through manual tagging of these objects with special property `Tags` which can contain multiple predefined values which also may be defined for customer profile
1. Inheritance `Tags` between catalogs objects depend on catalog taxonomy (categories inherit this property from parent catalog or category, products inherit this property from parent category)
1. Allow using `User groups` from customer profile to products or categories filtration in the storefront search
1. Different policies for `Tags` propagation in catalog objects hierarchy:
    1. `UpTree` propagate the tags from descendants to parents up the hierarchy
    ![image](https://user-images.githubusercontent.com/7566324/62931481-ba630c00-bdbe-11e9-9cdf-6d05e955721b.png);
    1. `DownTree` inherits the tags by descendants from their parent down the hierarchy
    ![image](https://user-images.githubusercontent.com/7566324/62931421-a3241e80-bdbe-11e9-8f02-fd22d0fbcc6f.png).

## Scenarios

### Create New User Group

1. Go to More->Contacts module-> select a user from the list and open user's details
1. 

### Assign User Group to Contact

1. Go to More->Contacts module and select a user from the list;
1. Open user's details;
1. Select the user group from the drop down;
1. Save the changes.


## Settings

Manage the tags propagation policies:
![image](https://user-images.githubusercontent.com/7566324/62931587-eaaaaa80-bdbe-11e9-8693-e1694f104f22.png)

### Example:

<details><summary>Managing catalog tags</summary><p>
  
![catalog-tags](https://user-images.githubusercontent.com/7536694/31497152-6510aac0-af5e-11e7-9e99-8f3f2d45cca3.gif) </p></details>

<details><summary>Managing category and product tags</summary><p>

![category-product-tags](https://user-images.githubusercontent.com/7536694/31497224-b5e4b1a8-af5e-11e7-90a1-918f120efddd.gif) </p></details>

#### Important! Don't forget to rebuild search indexes for categories and products after tagging process.

<details><summary>Rebuild indexes</summary><p>

![indexation](https://user-images.githubusercontent.com/7536694/31497248-d12a5efe-af5e-11e7-8938-b39d2b03ad1e.gif) </p></details>

<details><summary>Managing user groups</summary><p>

![user-groups](https://user-images.githubusercontent.com/7536694/31497349-252148ce-af5f-11e7-9c4b-a2c9545fd853.gif) </p></details>

##### As the result, products are displayed to the user only from the category that belongs to his `User group`.

<details><summary>Result view</summary><p>

![result](https://user-images.githubusercontent.com/7536694/31497775-53d994a4-af60-11e7-8544-90ccdf32f82a.gif) </p></details>


# Installation
Installing the module:
* Automatically: in VC Manager go to Configuration -> Modules -> Catalog personalization module -> Install
* Manually: download module zip package from https://github.com/VirtoCommerce/vc-module-catalog-personalization/releases. In VC Manager go to Configuration -> Modules -> Advanced -> upload module package -> Install.

# License
Copyright (c) Virto Solutions LTD.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

http://virtocommerce.com/opensourcelicense

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

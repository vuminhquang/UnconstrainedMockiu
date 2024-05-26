# Unconstrained Mockiu

Mock anything you want, static, private, SEALED, ...

Upon various online developer forums and question and answer sites, I have observed discussions concerning the mocking of static and sealed, etc.
Many responses indicated it was not possible, and questioners were sometimes challenged to justify their need.
However, the primary purpose of unit tests is to validate functionality in isolation.
-> THERE IS NO REASON TO REDESIGN A SOFTWARE TO MAKE IT BE TESTABLE (according to some xxx testing rules of others), that is truly silly.

This library aims to respect developers' testing freedoms by enabling the mocking of methods irrespective of their declared access or implementation.
It may prove useful for scenarios where traditional mocking approaches cannot be applied yet isolation and repeatability remain priorities.
-> If you respect your FREEDOM in testing, you can use this library.
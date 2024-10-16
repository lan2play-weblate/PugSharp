Release process / Branches
==================================================

.. note::
   This part of the documentation is only relevant for people that have full access on our repository, so most likeley not for you.

Introduction / Alpha
----------------------------------------

Currently we are developing on the ``main`` branch, which also builds alpha versions on every push automatically. 
In the future it will be changed, so every feature gets its own feature branch and main will only accept pull requests.

Beta
----------------------------------------

To deploy a beta release:

* go to `draft a release <https://github.com/Lan2Play/PugSharp/releases/new>`_ on the PugSharp Repository
* decide on the next release version number based on semver defaults
* create a tag with the naming scheme ``vX.X.X-beta`` (where X are numbers)
* enter the title with the naming scheme ``vX.X.X-beta`` (where X are numbers)
* you can then klick on ``Generate release notes`` and add the other changes in the description
* check ``Set as a pre-release`` 
* klick on Publish release
* the CI will add the nessecary files to the release

Stable
----------------------------------------

To deploy a beta release:

* go to `draft a release <https://github.com/Lan2Play/PugSharp/releases/new>`_ on the PugSharp Repository
* decide on the next release version number based on semver defaults
* create a tag with the naming scheme ``vX.X.X`` (where X are numbers)
* enter the title with the naming scheme ``vX.X.X`` (where X are numbers)
* you can then klick on ``Generate release notes`` and add the other changes in the description
* klick on Publish release
* the CI will add the nessecary files to the release
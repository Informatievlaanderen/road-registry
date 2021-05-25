# Releases

Each merged PR / branch results in a release (if the commit messages contain `fix:` or `feat:` though I'm not 100% sure this still holds). It's these releases that get pushed to the staging and production environment's docker repositories.
The associated release numbers result in docker tags. It's these tags that can be used by the infrastructure as code (terraform) to choose the version to deploy.

# Release Process

- Deploy the infrastructure (not part of this repository), if there are any changes to the infrastructure or if it's the first time
- Build a version (side effect of merging a PR or branch)
- Push docker images (side effect of building a version)

## Initial seeding

At this stage you will end up with an empty environment (either staging / production). For the road registry environment to be useful we need the data associated with the existing registry. This is where `extraction` comes into play. 
Using the `RoadRegistry.Legacy.Extract` project we can transform a legacy database into a `import-streams.zip` zip archive file containing a `streams.json` file with all new streams and associated events to be imported into the new environment. 
This archive needs to be uploaded into an S3 bucket which the `import` process, represented by `RoadRegistry.Legacy.Import`, is familiar with. The `import` process is **NOT** incremental, meaning it will only be executed once.
You can use the AWS CLI to perform the upload or you can configure the `extract` process to do the upload directly. Once the upload is performed the `import` process, running in the AWS environment, once every hour, will pick it and perform the actual import.
The import process consists of opening the `import-streams.zip` file and the `streams.json` file within it, and pumping the events into the corresponding streams. Two marker events indicate when the import has started and when it has completed.
As the import proceeds, the various projections will start projecting the imported events. Note this is suboptimal since processing 1 event at a time is going to take a very long time.
It's preferred to restart the projection hosts in order for them to run in catchup mode which is going to speed things up. Once all projections have caught up (WMS, Editor, Product, Command Host and Event Host), you can start performing uploads and downloads for editing and product purposes.


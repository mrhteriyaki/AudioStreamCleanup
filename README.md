## Audio Stream Cleanup
Reduce media storage by removing secondary audio streams from Video files.

Utility will scan folder.
Request selection of primary audio stream (default will be highest quality and english).
Processing will run ffmpeg on each file and exclude all audio streams except selected.

Results in smaller file size and removes non-english streams where default.

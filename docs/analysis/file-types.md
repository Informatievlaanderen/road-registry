# Introduction

This is an overview of all the file types involved in working with the road registry.

## .atx

Stores ArcGIS 8 attribute indexes for each shape and dbase file

## .shp

Stores the various road registry shapes / geometries, i.e. road nodes, road segments and road reference points

## .shx

Stores an index pointing to each record in a `.shp` file.

## .dbf

- dBASE III
- Stores records as a set of fields / attributes associated with a shape / geometry.
- Stores records as a set of fields / attributes, comparable to a regular database table.

## .prj

Stores information related to the coordinate system in use (related to the spatial reference identifier (SRID))

## Shape Files

The combination of a `.shp`, `.shx` and `.dbf` file.
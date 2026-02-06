# Highlights

The car is entirely physics driven using a distance-spring system and Unity's RigidBody `AddForceAtPosition()`.

An object pool is used to mitigate the performance impact of generating and deleting a large number of the collectables/obstacles.

The game world is entirely procedurally generated. A Bézier curve is generated using discrete anchor points. Points are generated using this curve, and from there, the road and mountains are formed around it.

The music was composed and produced by myself.

# Features/How it works
## Car Implementation
Force is applied to the body of the car (per wheel) in 4 parts:
- Acceleration (if gas is pressed)
	- Acts within the forward direction of the wheel
	- Can act forward/backward (accelerate/reverse)
- Suspension
	- Acts within the upward direction of the wheel
	- Basic distance-spring system; pushes the car up away from the ground
- Friction
	- Acts opposite to the overall velocity of the car *at the position of the wheel*
- Anti-slip
	- Acts within the side direction of the wheel
	- Helps to maintain drift and prevent the car from slipping out

## Object Pooling
ObjectPool is a generic class, each instance of ObjectPool stores a reference to the IPoolable object and a Stack containing references to all the available objects in the pool.

Pools are instantiated with the object to pool and an initial size. Using this it prepools a number of objects for it to use by calling `CreateNewObject(...)`.

`CreateNewObject(...)` is self-explanatory, it creates a new object and can optionally add it to the pool, or just return it for immediate use.

`GetObjectFromPool()` activates and returns an available object from the pool, or creates a new object if none are available.

`ReturnToPool(...)` disables an object and adds it back to the pool stack.

Object pools are used for the Coins, Crates, and Powerups.

## Generating The World
### Calculating Bézier curve points
- The road is based on a standard cubic Bézier curve.
- The curve is calculated from a start and end point, and 2 automatically calculated intermediate anchor points.
- A rough precision is calculated from the distance between the start and end point.
- This precision is used to figure out how many curve points get calculated.
### Calculating directions
- Then, some directions are calculated for each point in the curve:
- The up direction starts off defaulted to the same as the world up (0, 1, 0).
- The forward direction is calculated as the direction from the current point and the next point.
- The side direction is calculated as the cross product between the forward and up directions.
- The up direction gets *re*calculated as the cross product of the forward and side directions.
- These points and directions are used to generate the meshes.
### Building vertex lists
- A generalised RoadMeshBuilder class is used to make building the meshes easier.
- For ease, we construct the vertices in 4 different lists\*, then cast to arrays later.
- The "overall mesh" is split into each side (left, top, right, bottom), with each given their own mesh (for correct normals later).
- In BuildVerts(), a quad is constructed for each curve point, using the directions we calculated earlier. Though some special logic is used if we are at the very first curve point for every segment that is not the first. In this case, we use an overload of BuildVerts() that uses the *last* quad of the previous segment as the *first* quad of the current segment. This ensures the vertices line up correctly.
- The points in the quad are copied across the relevant (and enabled) side lists to construct each side of the mesh.
- After each curve point has been iterated over, and we have the lists for each side, we can start to stitch the tris.
### Building normals
- Normals are easy, we can just set the normal of each vertex for each side using the same directions as before.
- Left side = -sideDir
- Top side = +upDir
- Right side = +sideDir
- Bottom side = -upDir
### Stitching tris
- Stitching triangles (tris) are also relatively easy. Meshes store their tris as a 1-dimensional array of indices pointing to each vertex. It doesn't directly reference the vertex array.
- For ease, we construct the tris in 4 different lists\*, then cast to arrays later.
- The front face of a tri is determined based on the order of its vertices in the array. Clockwise means it's facing towards the camera, anticlockwise it isn't.
- Like I said before, tris are stored in a 1D array, but each tri is calculated by the engine as every 3 indices in the array. i.e. A tri array \[0, 3, 1, 0, 2, 3\] forms 2 tris using vertices \[0, 3, 1\] and \[0, 2, 3\].
- So, for every *index* of curve point (not their actual position in space) that isn't the first (>0), we stitch between the current indices and previous indices according to some predefined maths that I worked out in advance.
### Finalising the mesh
- At the end, we cast all the lists for each side into arrays, and assign them to each array of the actual meshes.
- We then collect all the side meshes and return it as a list to TrackPiece.
- TrackPiece then invokes an event which tells TrackGenerator to add the meshes to the world.

# Credits
## Textures
- Mountain texture 1: https://polyhaven.com/a/rocks_ground_04
- Mountain texture 2: https://polyhaven.com/a/rocky_trail
- Mountain texture 3: https://polyhaven.com/a/snow_02
- Mountain texture 4: https://polyhaven.com/a/snow_field_aerial

- Road texture: https://polyhaven.com/a/asphalt_track
- Road barrier texture: https://polyhaven.com/a/concrete_wall_007
- Star texture: https://texturelabs.org/textures/sky_143/

## Sounds
- https://pixabay.com/sound-effects/crate-break-1-93926/
- https://pixabay.com/sound-effects/wood-crate-destory-2-97263/
- https://pixabay.com/sound-effects/wooden-crate-smash-4-387903/
- https://pixabay.com/sound-effects/table-smash-47690/

## Tutorials adapted from
- https://www.patreon.com/posts/shader-graph-39024186
- https://www.youtube.com/watch?v=RF04Fi9OCPc
- https://www.youtube.com/watch?v=n_RHttAaRCk
- https://www.youtube.com/watch?v=nNmFLWup4_k
- https://www.youtube.com/watch?v=-UXIQKbl5RU
- https://www.youtube.com/watch?v=Q12sb-sOhdI

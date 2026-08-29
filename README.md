# Unity Tube Renderer
3D version of unity LineRenderer

`TubeRenderer` is a little bit of a misnomer since it depends on the `MeshRenderer`, and `MeshFilter`, built-in components to do the actual rendering.

## Purpose
* Move this fairly simple utility out of larger more complicated libraries which do this as a secondary or tertiary goal.
* To allow for scripted objects to display 3-dimensional paths and lines (such as a simulated rope, dynamic cables, or pipes)
* To allow detail level to be configured with scripting

### Using Git

Make sure the Git client is installed on your marchine and that you have added the Git executable path to your `PATH` environment variable.

Navigate to `%ProjectFolder%/Packages/` and open the `manifest.json` file.

in the "dependencies" section add:

```json
{
  "dependencies": {
      ...
      "com.github.oparaskos.unity.tube.renderer": "git+https://github.com/gallinator/unity-tube-renderer.git",
      ...
  }
}
```

Find more information about this [here](https://docs.unity3d.com/Manual/upm-git.html).

## Improvements from original
 - Fixed error when adding the `TubeRenderer` component to a `GameObject`
 - `MeshFilter` is created automatically
 - Support material assignment from inspector (defaults to URP Lit)
 - Support world space paths
 - Fix UV mapping seams with correct UV scale
 - Editor only tube editing

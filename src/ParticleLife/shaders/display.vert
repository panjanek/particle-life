#version 430

struct Particle
{
   vec2 position;
   vec2 velocity;
   int species;
   int flags;
};

layout(std430, binding = 3) buffer OutputBuffer {
    Particle points[];
};

uniform mat4 projection;

layout(location=0) out vec3 vColor;

void main()
{
    uint id = gl_VertexID;
    gl_Position = projection * vec4(points[id].position, 0.0, 1.0);
    gl_PointSize = 1.0;

    if (points[id].flags == 1)
        gl_PointSize = 2.0;

    // Extract zoom factor from projection
    float zoomX = length(projection[0].xy);
    float zoomY = length(projection[1].xy);
    float zoom  = max(zoomX, zoomY);

    gl_PointSize *= zoom*1920;

    uint spec = points[id].species;
    const vec3 colors[] = vec3[](
        vec3(1.0, 0.0, 0.0), // red
        vec3(0.0, 1.0, 0.0), // green
        vec3(0.0, 0.0, 1.0), // blue
        vec3(1.0, 1.0, 0.0), // yellow
        vec3(1.0, 0.0, 1.0), // magenta
        vec3(0.0, 1.0, 1.0), // cyan
        vec3(1.0, 1.0, 1.0)  // white
    );

    vColor = colors[spec%7];
}
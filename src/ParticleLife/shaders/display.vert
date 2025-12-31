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
uniform vec2 viewportSize;

layout(location=0) out vec3 vColor;

float zoomFromProjection()
{
    float sx = abs(projection[0][0]);
    float sy = abs(projection[1][1]);
    return max(sx, sy) * max(viewportSize.x, viewportSize.y) * 0.5;
}

void main()
{
    uint id = gl_VertexID;
    gl_Position = projection * vec4(points[id].position, 0.0, 1.0);
    gl_PointSize = 2.0;

    if (points[id].flags == 1)
        gl_PointSize = 4.0;

    float zoom = zoomFromProjection();
    gl_PointSize *= zoom;

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
#version 430

struct Particle
{
   vec2 position;
   vec2 velocity;
   uint species;
   int flags;
};

layout(std430, binding = 3) buffer OutputBuffer {
    Particle points[];
};

uniform mat4 projection;
uniform vec2 viewportSize;
uniform float paricleSize;

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

    float baseSize = paricleSize;
    if (baseSize == 0)
        baseSize = 2.0;

    gl_PointSize = baseSize;

    if (points[id].flags == 1)
        gl_PointSize = baseSize*1.5;

    float zoom = zoomFromProjection();
    gl_PointSize *= zoom;

    uint spec = points[id].species;
    const vec3 colors[] = vec3[](
        vec3(1.0, 1.0, 0.0), // yellow
        vec3(1.0, 0.0, 1.0), // magenta
        vec3(0.0, 1.0, 1.0), // cyan
        vec3(1.0, 0.0, 0.0), // red
        vec3(0.0, 1.0, 0.0), // green
        vec3(0.0, 0.0, 1.0), // blue
        vec3(1.0, 1.0, 1.0), // white
        vec3(0.5, 0.5, 0.5)  // gray
    );

    vColor = colors[spec%8];
}
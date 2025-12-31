#version 430

struct Particle
{
   vec2 position;
   vec2 velocity;
   int species;
   int _pad;   // padding to 24 bytes
};

layout(std430, binding = 1) buffer OutputBuffer {
    Particle points[];
};

uniform mat4 projection;

layout(location=0) out vec3 vColor;

void main()
{
    uint id = gl_VertexID;
    gl_Position = projection * vec4(points[id].position, 0.0, 1.0);
    gl_PointSize = 5.0;
    vColor = vec3(1.0, 1.0, 1.0);
}
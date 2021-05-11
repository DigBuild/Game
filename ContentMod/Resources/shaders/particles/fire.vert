#version 450

layout(binding = 0) uniform UBO {
	mat4 matrix;
    mat4 flattenMatrix;
};

layout(location = 0) in vec3 vertPos;

layout(location = 1) in vec3 pos;
layout(location = 2) in float age;

layout(location = 0) out vec2 fragUV;
layout(location = 1) out flat float fragAge;

void main() {
    float scale = (1 - age) * (1 - age);
    gl_Position = matrix * (vec4(pos, 0.0) + flattenMatrix * vec4(vertPos * scale * 0.2, 1.0));
    fragUV = vertPos.xy + 0.5;
    fragAge = age;
}

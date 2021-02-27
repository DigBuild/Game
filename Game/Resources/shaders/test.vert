#version 450

layout(binding = 0) uniform UBO {
	mat4 matrix;
};
layout(set = 1, binding = 1) uniform UBO2 {
	mat4 matrix2;
};

layout(location = 0) in vec3 pos;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec2 uv;

layout(location = 0) out vec2 fragUV;
layout(location = 1) out vec3 fragNormal;

void main() {
    gl_Position = matrix2 * matrix * vec4(pos.xyz, 1.0);
    fragUV = uv;
	fragNormal = normal;
}
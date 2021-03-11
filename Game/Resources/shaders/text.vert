#version 450

layout(binding = 0) uniform UBO {
	mat4 matrix;
};

layout(location = 0) in vec2 pos;
layout(location = 1) in vec2 uv;

layout(location = 0) out vec2 fragUV;

void main() {
    gl_Position = matrix * vec4(pos, 0.0, 1.0) - vec4(1.0, 1.0, 0.0, 0.0);
    fragUV = uv;
}

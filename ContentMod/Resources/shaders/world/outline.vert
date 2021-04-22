#version 450

layout(binding = 0) uniform UBO {
	mat4 matrix;
};

layout(location = 0) in vec3 pos;

void main() {
    gl_Position = matrix * vec4(pos.xyz, 1.0);
}

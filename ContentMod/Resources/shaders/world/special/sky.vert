#version 450

layout(binding = 0) uniform UBO {
	mat4 matrix;
};

layout(location = 0) in vec2 pos;

layout(location = 0) out vec3 fragPosition;
layout(location = 1) out vec3 fragNormal;

void main() {
    gl_Position = vec4((pos - vec2(0.5, 0.5)) * 2, 0.0, 1.0);
    gl_Position.z = (gl_Position.z + gl_Position.w) / 2.0;
	fragPosition = (gl_Position.xyz) * vec3(0.5, 0.5, 1) + vec3(0.5, 0.5, 0);
    fragNormal = (matrix * vec4(pos * 2 - 1, 1, 0)).xyz;
}

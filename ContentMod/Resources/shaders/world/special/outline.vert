#version 450

layout(binding = 0) uniform UBO {
	mat4 modelView;
	mat4 projection;
};

layout(location = 0) in vec3 pos;

void main() {
    gl_Position = projection * modelView * vec4(pos.xyz, 1.0);
    gl_Position.z = (gl_Position.z + gl_Position.w) / 2.0;
}

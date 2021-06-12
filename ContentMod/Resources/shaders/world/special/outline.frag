#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(location = 0) out vec4 outColor;
layout(location = 1) out vec4 outBloomColor;
layout(location = 2) out vec4 outWater;
layout(location = 3) out vec4 outNormal;
layout(location = 4) out vec4 outPosition;

void main() {
    outColor = vec4(0.0, 0.0, 0.0, 1.0);
    outBloomColor = vec4(0.0, 0.0, 0.0, 1.0);
    outWater = outNormal = outPosition = vec4(0);
}

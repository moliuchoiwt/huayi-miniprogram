<template>
  <div class="h100">
    <el-tabs type="border-card" class="h100">
      <div class="scroll h100">
        <el-form ref="FormRef" :model="configData" label-position="top">
          <el-tab-pane label="小程序配置">
            <el-form-item label="平台名称" prop="SiteName">
              <el-input v-model="configData.SiteName" show-word-limit maxlength="50" />
            </el-form-item>
            <el-form-item label="平台域名" prop="DomianName">
              <el-input v-model="configData.DomianName" show-word-limit maxlength="50" />
            </el-form-item>
            <el-form-item label="平台资源域名" prop="DomianStaticName">
              <el-input v-model="configData.DomianStaticName" show-word-limit maxlength="50" />
            </el-form-item>
            <el-form-item label="平台Logo" prop="SiteLogo">
              <UploadImg v-model:image-url="configData.SiteLogo"></UploadImg>
            </el-form-item>
            <!-- <el-form-item label="首页视频" prop="videoUrl">
              <Video v-model:url="configData.videoUrl"></Video>
            </el-form-item> -->
            <el-form-item label="公司名称" prop="WebCompany">
              <el-input v-model="configData.WebCompany" show-word-limit maxlength="50" />
            </el-form-item>
            <el-form-item label="公司地址" prop="WebAddress">
              <el-input v-model="configData.WebAddress" :autosize="{ minRows: 3, maxRows: 5 }" type="textarea" />
            </el-form-item>
            <el-form-item label="联系方式" prop="CustomerInfo">
              <el-input v-model="configData.CustomerInfo" show-word-limit maxlength="50" />
            </el-form-item>
            <el-form-item label="微信审核开关">
              <el-switch v-model="configData.MpWxOpenCheck" />
            </el-form-item>
            <el-form-item label="小程序审核版本号" prop="MpWxOpenVersion" v-if="configData.MpWxOpenCheck">
              <el-input v-model="configData.MpWxOpenVersion" show-word-limit maxlength="50" />
            </el-form-item>
          </el-tab-pane>
          <el-tab-pane label="平台配置">
            <el-form-item label="订单最小金额" prop="orderMinPrice">
              <el-input-number v-model="configData.orderMinPrice" :precision="0" :step="1" :min="1" />
            </el-form-item>
            <el-form-item label="平台订单抽成比例" prop="PlatformProportion">
              <el-input-number
                v-model="configData.PlatformProportion"
                :precision="2"
                :max="1"
                :step="0.01"
                :min="0"
                :value-on-clear="0"
              />
              <span style="margin-left: 10px; color: red">{{ (configData.PlatformProportion * 100).toFixed(2) }}%</span>
            </el-form-item>
            <el-form-item label="任务发布提示" prop="textContents">
              <el-input type="textarea" v-model="configData.textContents" show-word-limit />
            </el-form-item>
          </el-tab-pane>
          <el-tab-pane label="提现配置">
            <el-form-item label="提现最小金额" prop="UserMinMoney">
              <el-input-number v-model="configData.UserMinMoney" :min="1" :value-on-clear="0" />
            </el-form-item>
            <el-form-item label="提现手续费比例" prop="UserWithdrawalRate">
              <el-input-number
                v-model="configData.UserWithdrawalRate"
                :precision="2"
                :max="1"
                :step="0.01"
                :min="0"
                :value-on-clear="0"
              />
              <span style="margin-left: 10px; color: red">{{ (configData.UserWithdrawalRate * 100).toFixed(2) }}%</span>
            </el-form-item>
            <el-form-item label="提现说明" prop="UserWithdrawIntro">
              <WangEditor v-model:value="configData.UserWithdrawIntro" height="400px" />
            </el-form-item>
          </el-tab-pane>
          <!-- <el-tab-pane label="隐私条例">
            <WangEditor v-model:value="configData.PrivacyInfo" height="400px" />
          </el-tab-pane> -->
          <el-tab-pane label="接单须知">
            <WangEditor v-model:value="configData.richText1" height="400px" />
          </el-tab-pane>
          <!-- <el-tab-pane label="公司介绍">
            <WangEditor v-model:value="configData.MemberIntro" height="400px" />
          </el-tab-pane> -->
          <el-tab-pane label="微信配置" v-if="isDevelopment">
            <el-form-item label="小程序AppId" prop="WxOpenAppId">
              <el-input v-model="configData.WxOpenAppId" />
            </el-form-item>
            <el-form-item label="AppSecret" prop="WxOpenAppSecret">
              <el-input type="password" ref="password" show-password v-model="configData.WxOpenAppSecret" />
            </el-form-item>
            <el-form-item label="商户号" prop="mch_id">
              <el-input v-model="configData.mch_id" />
            </el-form-item>
            <el-form-item label="商户号秘钥" prop="mch_idkey">
              <el-input type="password" ref="password" show-password v-model="configData.mch_idkey" />
            </el-form-item>
            <el-form-item label="商户号证书" prop="certPath">
              <el-input v-model="configData.certPath" />
            </el-form-item>
            <el-form-item label="商户号证书秘钥" prop="certPwd">
              <el-input type="password" ref="password" show-password v-model="configData.certPwd" />
            </el-form-item>
            <el-form-item label="商户号私钥路径" prop="privateKeyPath">
              <el-input v-model="configData.privateKeyPath" />
            </el-form-item>
            <el-form-item label="快递100授权key" prop="KuaiDi100Key">
              <el-input v-model="configData.KuaiDi100Key" />
            </el-form-item>
            <el-form-item label="快递100Customer" prop="KuaiDi100Customer">
              <el-input v-model="configData.KuaiDi100Customer" />
            </el-form-item>
          </el-tab-pane>
        </el-form>
        <div style="margin-top: 1.25rem; text-align: center">
          <el-button type="primary" @click="onSubmit()">确定</el-button>
        </div>
      </div>
    </el-tabs>
  </div>
</template>

<script setup lang="ts">
import { sysUserApi } from "@/api/api";
import UploadImg from "@/components/Upload/Img.vue";
// import Video from "@/components/Upload/Video.vue";
import WangEditor from "@/components/WangEditor/index.vue";
import { ElMessage } from "element-plus";
import { ref } from "vue";
let isDevelopment = import.meta.env.VITE_USER_NODE_ENV == "development";
let configData = ref<any>([]);
sysUserApi.GetSysConfig().then(res => {
  configData.value = res.data;
});
// 提交
const onSubmit = async () => {
  let data = { ...configData.value };
  const res = await sysUserApi.OperationSysConfig(data);
  if (res.code == "200") {
    ElMessage.success("操作成功");
  }
};
</script>

<style lang="scss" scoped>
.h100 {
  height: 100%;
}
.scroll {
  padding: 0 20px;
  overflow: scroll;
}
</style>

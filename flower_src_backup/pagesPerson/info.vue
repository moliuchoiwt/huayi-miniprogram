<template>
	<view class="page">
		<button class="item allbutton" open-type="chooseAvatar" @chooseavatar="chooseAvatar">
			<view class="left">头像</view>
			<view class="right">
				<image :src="baseImgUrl(storeUser.user.avatar) || '/static/logo.png'" mode="aspectFill"></image>
				<view><up-icon name="arrow-right" color="#969696" size="20"></up-icon></view>
			</view>
		</button>
		<view class="item" @click="nickTemp = storeUser.user.nickName; nickShow = true">
			<view class="left">昵称</view>
			<view class="right">
				{{ storeUser.user.nickName }}
				<view><up-icon name="arrow-right" color="#969696" size="20"></up-icon></view>
			</view>
		</view>
		<view class="item">
			<view class="left">性别</view>
			<view class="right">
				<up-radio-group v-model="storeUser.user.gender" placement="row" @change="change">
					<up-radio activeColor="#8C4FFF" label="保密" name=""></up-radio>
					<up-radio activeColor="#8C4FFF" label="男" name="男"></up-radio>
					<up-radio activeColor="#8C4FFF" label="女" name="女"></up-radio>
				</up-radio-group>
			</view>
		</view>
		<view class="item">
			<view class="left">地区</view>
			<view class="right">
				<picker mode="region" class="u-w-440" @change="bindTimeChange">
					<up-input placeholder="请选择省市区" inputAlign="right" type="text" readonly v-model="addressName"
						border="none" />
				</picker>
				<view><up-icon name="arrow-right" color="#969696" size="20"></up-icon></view>
			</view>
		</view>
		<view class="item" @click="addressTemp = storeUser.user.address; addressNickShow = true">
			<view class="left">详情地址</view>
			<view class="right">
				{{ storeUser.user.address }}
				<view><up-icon name="arrow-right" color="#969696" size="20"></up-icon></view>
			</view>
		</view>
		<view class="item">
			<view class="left">个人介绍</view>
			<view class="right"></view>
		</view>
		<up-textarea placeholder="请输入个人介绍" v-model="storeUser.user.intro" @change="change"></up-textarea>
		<view class="btns">
			<view class="btn" @click="unLogin">退出登录</view>
		</view>
		<!-- 修改昵称 -->
		<up-modal :show="nickShow" title="编辑昵称" @cancel='nickShow = false' @close='nickShow = false' closeOnClickOverlay
			@confirm='nickConfirm' showCancelButton>
			<up-input v-model="nickTemp"></up-input>
		</up-modal>
		<!-- 修改详情地址 -->
		<up-modal :show="addressNickShow" title="编辑详情地址" @cancel='addressNickShow = false' @close='addressNickShow = false'
			closeOnClickOverlay @confirm='addressConfirm' showCancelButton>
			<up-input v-model="addressTemp"></up-input>
		</up-modal>
	</view>
</template>

<script>
const d = new Date()
const year = d.getFullYear()
let month = d.getMonth() + 1
month = month < 10 ? `0${month}` : month
const date = d.getDate()

export default {
	data() {
		return {
			birthdayShow: false,
			nickShow: false,
			nickTemp: '',
			addressName: "",
			addressNickShow: false,
			addressTemp: '',
		}
	},
	onLoad() {
		this.addressName = this.storeUser.user.province + '' + this.storeUser.user.city + '' + this.storeUser.user.area
	},
	methods: {
		bindTimeChange(e) {
			this.storeUser.user.province = e.detail.value[0]
			this.storeUser.user.city = e.detail.value[1]
			this.storeUser.user.area = e.detail.value[2]
			this.addressName = this.storeUser.user.province + '' + this.storeUser.user.city + '' + this.storeUser.user
				.area
			this.request('User/OperationUser', this.storeUser.user)
		},
		// 选择头像
		async chooseAvatar(e) {
			const {
				avatarUrl
			} = e.detail
			const res = await this.uploadFile(avatarUrl)
			let user = {
				...this.storeUser.user
			}
			user.avatar = res
			await this.request('User/OperationUser', user)
			this.storeUser.updateUser()
		},
		nickConfirm() {
			this.storeUser.user.nickName = this.nickTemp
			this.request('User/OperationUser', this.storeUser.user)
			this.nickShow = false
		},
		addressConfirm() {
			this.storeUser.user.address = this.addressTemp
			this.request('User/OperationUser', this.storeUser.user)
			this.addressNickShow = false
		},
		change() {
			this.request('User/OperationUser', this.storeUser.user)
		},
		// 退出登录
		unLogin() {
			this.modal('退出登录', '确定退出登录吗?', callback => {
				if (callback) {
					uni.clearStorageSync()
					uni.redirectTo({
						url: '/pages/login/index'
					})
				}
			})
		},
	}
}
</script>

<style lang="scss" scoped>
.page {
	background: white;
	min-height: 100vh;
	padding: 0 32rpx;
}

.allbutton {
	padding: 0;
	background-color: initial;
	margin: 0;
	line-height: initial;
	overflow: initial;
	box-sizing: initial;
	font-size: initial;
	outline: none;
}

.allbutton::after {
	content: none;
}

.item {
	padding: 20rpx 0;
	display: flex;
	justify-content: space-between;
	align-items: center;

	.left {
		font-size: 30rpx;
		color: #333333;
	}

	.right {
		display: flex;
		align-items: center;
		font-size: 30rpx;
		color: #333333;

		image {
			width: 80rpx;
			height: 80rpx;
			border-radius: 50%;
		}

		>view {
			margin-left: 20rpx;
		}
	}
}

.btns {
	margin-top: 397rpx;
	display: flex;
	justify-content: center;

	.btn {
		width: 523rpx;
		height: 80rpx;
		line-height: 80rpx;
		text-align: center;
		border: 2rpx solid #333333;
		border-radius: 40rpx;
		font-size: 32rpx;
		color: #333333;
	}
}
</style>